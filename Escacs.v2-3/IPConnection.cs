using System.Net.Sockets;

namespace Chess
{
    class IPConnection
    {
        private TcpClient _client;

        public IPConnection()
        {
            _client = new TcpClient();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                if (!_client.Connected)
                {
                    _client.Connect(ipAddress, port);
                }
                return true;
            }
            catch (SocketException)
            {
                // Handle exceptions as needed, e.g., log or notify the user.
                return false;
            }
        }

        public void SendData(string data)
        {
            if (_client.Connected)
            {
                var stream = _client.GetStream();
                byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(data);
                stream.Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public string ReceiveData()
        {
            if (_client.Connected)
            {
                var stream = _client.GetStream();
                byte[] bytesToRead = new byte[_client.ReceiveBufferSize];
                int bytesRead = stream.Read(bytesToRead, 0, _client.ReceiveBufferSize);
                return System.Text.Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            }
            return null;
        }

        public void Disconnect()
        {
            if (_client.Connected)
            {
                _client.Close();
            }
        }
    }
}
