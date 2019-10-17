using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HMAC
{
    class TCP_Socket
    {
        private string lasterror;
        private string server;
        private int port;
        private int providerID;
        private Socket s;
        ASCIIEncoding encoding = new ASCIIEncoding();

        public TCP_Socket()
        {

        }

        public TCP_Socket(string Server, int Port, int ProviderID)
        {
            server = Server;
            port = Port;
            providerID = ProviderID;
        }

        public string LastError
        {
            get { return this.lasterror; }
            set { this.lasterror = value; }
        }

        public string Server
        {
            get { return this.server; }

            set { this.server = value; }
        }
        public int Port
        {
            get { return this.port; }

            set { this.port = value; }
        }
        public int ProviderID
        {
            get { return this.providerID; }

            set { this.providerID = value; }
        }

        //= "127.0.0.1";//"globetech.gps.be-mobile.biz";
        //int port = 10001;//8085;
        public byte[] toBytes(string VehicleID, int GPS_Time, int LAT, int LON, short Speed = 0, short heading = 0, byte Enginestate = 0xFF, short Vehicle_type = 0, bool toLittleIndian = false)
        {
            #region Header
            byte[] header = new byte[6];
            // Start Of Message 1 byte
            byte[] SOM_byte = { 0xF7 };

            // Provider ID 2 bytes
            byte[] ProviderID_byte = BitConverter.GetBytes((short)providerID);

            // Number Of Body Records 2 bytes
            byte[] NOBR_byte = BitConverter.GetBytes((short)1);

            // Future Use 1 byte
            byte[] Future_byte = { 0x00 };

            int len = 0;
            if (toLittleIndian)
                Array.Reverse(SOM_byte);
            System.Buffer.BlockCopy(SOM_byte, 0, header, len, SOM_byte.Length);
            len += SOM_byte.Length;

            if (toLittleIndian)
                Array.Reverse(ProviderID_byte);
            System.Buffer.BlockCopy(ProviderID_byte, 0, header, len, ProviderID_byte.Length);
            len += ProviderID_byte.Length;

            if (toLittleIndian)
                Array.Reverse(NOBR_byte);
            System.Buffer.BlockCopy(NOBR_byte, 0, header, len, NOBR_byte.Length);
            len += NOBR_byte.Length;

            if (toLittleIndian)
                Array.Reverse(Future_byte);
            System.Buffer.BlockCopy(Future_byte, 0, header, len, Future_byte.Length);


            #endregion

            #region Data
            // VehicleID 20 bytes
            byte[] VehicleID_byte = new byte[20];
            var stringInBytes = Encoding.ASCII.GetBytes(VehicleID);
            System.Buffer.BlockCopy(stringInBytes, 0, VehicleID_byte, 0, stringInBytes.Length);

            // GPS_TIME 4 bytes
            byte[] GPS_Time_byte = BitConverter.GetBytes(GPS_Time);

            // LATITUDE 4 bytes
            byte[] LAT_byte = BitConverter.GetBytes(LAT);

            // LONTITUDE 4 bytes
            byte[] LON_byte = BitConverter.GetBytes(LON);

            // Speed 2 bytes
            byte[] Speed_byte = BitConverter.GetBytes(Speed);

            // Heading 2 bytes
            byte[] heading_byte = BitConverter.GetBytes(heading);

            // Enginestate 1 byte
            byte[] Enginestate_byte = BitConverter.GetBytes(Enginestate);

            // Vehicle_type 2 bytes
            byte[] Vehicle_type_byte = BitConverter.GetBytes(Vehicle_type);

            // Combine
            byte[] data = new byte[VehicleID_byte.Length + GPS_Time_byte.Length + LAT_byte.Length + LON_byte.Length + Speed_byte.Length + heading_byte.Length + 1 + Vehicle_type_byte.Length];
            len = 0;

            System.Buffer.BlockCopy(VehicleID_byte, 0, data, len, VehicleID_byte.Length);
            len += VehicleID_byte.Length;

            if (toLittleIndian)
                Array.Reverse(GPS_Time_byte);
            System.Buffer.BlockCopy(GPS_Time_byte, 0, data, len, GPS_Time_byte.Length);
            len += GPS_Time_byte.Length;

            if (toLittleIndian)
                Array.Reverse(LAT_byte);
            System.Buffer.BlockCopy(LAT_byte, 0, data, len, LAT_byte.Length);
            len += LAT_byte.Length;

            if (toLittleIndian)
                Array.Reverse(LON_byte);
            System.Buffer.BlockCopy(LON_byte, 0, data, len, LON_byte.Length);
            len += LON_byte.Length;

            if (toLittleIndian)
                Array.Reverse(Speed_byte);
            System.Buffer.BlockCopy(Speed_byte, 0, data, len, Speed_byte.Length);
            len += Speed_byte.Length;

            if (toLittleIndian)
                Array.Reverse(heading_byte);
            System.Buffer.BlockCopy(heading_byte, 0, data, len, heading_byte.Length);
            len += heading_byte.Length;

            if (toLittleIndian)
                Array.Reverse(Enginestate_byte);
            System.Buffer.BlockCopy(Enginestate_byte, 1, data, len, 1);
            len += 1;

            if (toLittleIndian)
                Array.Reverse(Vehicle_type_byte);
            System.Buffer.BlockCopy(Vehicle_type_byte, 0, data, len, Vehicle_type_byte.Length);
            len += Vehicle_type_byte.Length;
            #endregion

            #region Footer
            // Message Terminator 2 bytes
            byte[] MT_byte = { 0x5A, 0x0A };
            #endregion

            byte[] result = new byte[header.Length + data.Length + MT_byte.Length];
            len = 0;
            System.Buffer.BlockCopy(header, 0, result, len, header.Length);
            len += header.Length;
            System.Buffer.BlockCopy(data, 0, result, len, data.Length);
            len += data.Length;
            System.Buffer.BlockCopy(MT_byte, 0, result, len, MT_byte.Length);
            len += MT_byte.Length;
            return result;
        }

        public bool Connect()
        {
            // Create a socket connection with the specified server and port.
            s = ConnectSocket(server, port);

            if (s == null)
                return false;
            else
                return true;
        }

        public bool IsConnect()
        {
            return s.Connected;
        }

        public bool Disconnect()
        {
            try
            {
                if (s.Connected)
                {
                    s.Disconnect(false);
                }
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;
        }
        public bool SendData(byte[] data, bool waitResponse, int timeoutSend = 60000)
        {
            try
            {
                // Send request to the server.
                s.SendTimeout = timeoutSend;
                s.Send(data, data.Length, 0);
                byte[] res_byte = new byte[100];
                if (waitResponse)
                {
                    s.Receive(res_byte);

                    string res = encoding.GetString(res_byte).Replace("\0", string.Empty);


                    if (res == "true")
                        return true;
                    else
                        return false;
                }

                return true;

            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }



        }

        public bool SendData(string data)
        {
            try
            {
                byte[] data_byte = encoding.GetBytes(data);
                // Send request to the server.
                s.Send(data_byte, data_byte.Length, 0);
                byte[] res_byte = new byte[100];
                s.Receive(res_byte);

                string res = encoding.GetString(res_byte).Replace("\0", string.Empty);


                if (res == "true")
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        public bool SendData(string data, bool waitResponse = false, int sendTimeOut = 0, int recieveTimeOut = 0)
        {
            try
            {
                byte[] data_byte = encoding.GetBytes(data);
                // Send request to the server.
                s.SendTimeout = sendTimeOut;
                s.Send(data_byte, data_byte.Length, 0);
                byte[] res_byte = new byte[100];
                if (waitResponse)
                {
                    s.ReceiveTimeout = recieveTimeOut;
                    s.Receive(res_byte);

                    string res = encoding.GetString(res_byte).Replace("\0", string.Empty);


                    if (res == "true")
                        return true;
                    else
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return false;
            }
        }

        public string recieve(int recieveTimeOut = 0)
        {
            try
            {
                byte[] res_byte = new byte[100];
                s.ReceiveTimeout = recieveTimeOut;
                s.Receive(res_byte);

                string res = encoding.GetString(res_byte).Replace("\0", string.Empty);
                return res;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                return null;
            }
        }

        private static void ProcessVehicleId(byte[] message, byte[] vehicleIdBytes)
        {
            System.Buffer.BlockCopy(vehicleIdBytes, 0, message, 0, vehicleIdBytes.Length);
            int k = 0;
            for (; k < 20 - vehicleIdBytes.Length - 1; k++)
            {
                //message.Add(0x00);
                message[k] = 0x00;
            }
            //message.Add(0x00);
            message[19] = 0x00;
        }

        private byte[] GetBytes(string str, int Size = 20)
        {
            byte[] bytes = new byte[Size];
            if (Size > str.Length * sizeof(char))
                System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, str.Length * sizeof(char));
            else
                System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        //private Socket ConnectSocket(string server, int port)
        //{
        //    if (String.IsNullOrEmpty(server))
        //    {
        //        throw new Exception("Server name cannot be null. Please initial server name or server IP.");
        //    }

        //    if (port == 0)
        //    {
        //        throw new Exception("port number cannot be 0. Please initail server port.");
        //    }

        //    if(providerID == 0)
        //    {
        //        throw new Exception("ProviderID cannot be 0. Please initail ProviderID.");
        //    }

        //    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    IPAddress remoteIPAddress = IPAddress.Parse(server);
        //    IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPAddress, port);
        //    s.Connect(remoteEndPoint);

        //    return s;
        //}

        private Socket ConnectSocket(string server, int port)
        {
            if (String.IsNullOrEmpty(server))
            {
                throw new Exception("Server name cannot be null. Please initial server name or server IP.");
            }

            if (port == 0)
            {
                throw new Exception("port number cannot be 0. Please initail server port.");
            }

            if (providerID == 0)
            {
                throw new Exception("ProviderID cannot be 0. Please initail ProviderID.");
            }

            try
            {
                Socket s = null;
                IPHostEntry hostEntry = null;

                // Get host related information.
                hostEntry = Dns.GetHostEntry(server);

                // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid 
                // an exception that occurs when the host IP Address is not compatible with the address family 
                // (typical in the IPv6 case). 
                foreach (IPAddress address in hostEntry.AddressList)
                {
                    IPEndPoint ipe = new IPEndPoint(address, port);
                    // Check IPv4?
                    if (ipe.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Create Socket TCP
                        Socket tempSocket =
                            new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                        tempSocket.Connect(ipe);

                        if (tempSocket.Connected)
                        {
                            s = tempSocket;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                return s;
            }
            catch (Exception ex)
            {
                //throw new Exception("Error : " + ex.Message);
                return null;
            }

        }
    }
}
