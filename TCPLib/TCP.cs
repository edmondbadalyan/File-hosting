using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;

namespace TCPLib
{
    public class TCP
    {
        #region Send
        public static async Task SendString(TcpClient client, string text)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            await client.GetStream().WriteAsync(BitConverter.GetBytes(bytes.Length));
            await client.GetStream().WriteAsync(bytes);
            await Console.Out.WriteLineAsync($"Sent string: {text}");
        }

        public static async Task SendString(TcpClient client, string text, CancellationToken token)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            await client.GetStream().WriteAsync(BitConverter.GetBytes(bytes.Length), token);
            await client.GetStream().WriteAsync(bytes, token);
            await Console.Out.WriteLineAsync($"Sent string: {text}");
        }

        public static async Task SendLong(TcpClient client, long number)
        {
            await client.GetStream().WriteAsync(BitConverter.GetBytes(number));
            await Console.Out.WriteLineAsync($"Sent long: {number}");
        }

        public static async Task SendLong(TcpClient client, long number, CancellationToken token)
        {
            await client.GetStream().WriteAsync(BitConverter.GetBytes(number), token);
            await Console.Out.WriteLineAsync($"Sent long: {number}");
        }

        public static async Task SendInt(TcpClient client, int number)
        {
            await client.GetStream().WriteAsync(BitConverter.GetBytes(number));
            await Console.Out.WriteLineAsync($"Sent int: {number}");
        }

        public static async Task SendInt(TcpClient client, int number, CancellationToken token)
        {
            await client.GetStream().WriteAsync(BitConverter.GetBytes(number), token);
            await Console.Out.WriteLineAsync($"Sent int: {number}");
        }


        public static async Task SendFile(TcpClient client, Stream file, long length)
        {
            await Console.Out.WriteLineAsync("Started sending file");

            await SendLong(client, length);

            byte[] buffer = new byte[1024];
            int pos = 0;
            while (pos < length)
            {
                int read = await file.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, length - pos));
                await client.GetStream().WriteAsync(buffer, 0, read);
                pos += read;
            }
            await Console.Out.WriteLineAsync("File sent successfully");
        }

        public static async Task SendFile(TcpClient client, Stream file, long length, CancellationToken token, IProgress<double> progress = null)
        {
            await Console.Out.WriteLineAsync("Started sending file");

            token.ThrowIfCancellationRequested();

            await SendLong(client, length, token);

            byte[] buffer = new byte[1024];
            int pos = 0;
            while (pos < length)
            {
                token.ThrowIfCancellationRequested();

                int read = await file.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, length - pos), token);
                await client.GetStream().WriteAsync(buffer, 0, read, token);
                pos += read;

                progress?.Report((double)pos / length * 100);
            }
            await Console.Out.WriteLineAsync("File sent successfully");
        }
        #endregion

        #region Receive
        public static async Task<byte[]> ReceiveFixed(TcpClient client, int length)
        {
            byte[] buffer = new byte[length];
            await client.GetStream().ReadAsync(buffer, 0, length);
            return buffer;
        }

        public static async Task<byte[]> ReceiveFixed(TcpClient client, int length, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            byte[] buffer = new byte[length];
            await client.GetStream().ReadAsync(buffer, 0, length, token);
            return buffer;
        }

        public static async Task<int> ReceiveInt(TcpClient client)
        {
            byte[] length_buffer = await ReceiveFixed(client, sizeof(int));
            await Console.Out.WriteLineAsync($"Received int: {BitConverter.ToInt32(length_buffer)}");
            return BitConverter.ToInt32(length_buffer);
        }

        public static async Task<int> ReceiveInt(TcpClient client, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            byte[] length_buffer = await ReceiveFixed(client, sizeof(int), token);
            await Console.Out.WriteLineAsync($"Received int: {BitConverter.ToInt32(length_buffer)}");
            return BitConverter.ToInt32(length_buffer);
        }

        public static async Task<long> ReceiveLong(TcpClient client)
        {
            byte[] length_buffer = await ReceiveFixed(client, sizeof(long));
            await Console.Out.WriteLineAsync($"Received long: {BitConverter.ToInt64(length_buffer)}");
            return BitConverter.ToInt64(length_buffer);
        }

        public static async Task<long> ReceiveLong(TcpClient client, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            byte[] length_buffer = await ReceiveFixed(client, sizeof(long), token);
            await Console.Out.WriteLineAsync($"Received long: {BitConverter.ToInt64(length_buffer)}");
            return BitConverter.ToInt64(length_buffer);
        }

        public static async Task<byte[]> ReceiveVariable(TcpClient client)
        {
            int length = await ReceiveInt(client);
            return await ReceiveFixed(client, length);
        }

        public static async Task<byte[]> ReceiveVariable(TcpClient client, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            int length = await ReceiveInt(client, token);
            return await ReceiveFixed(client, length, token);
        }

        public static async Task<string> ReceiveString(TcpClient client)
        {
            byte[] string_buffer = await ReceiveVariable(client);
            await Console.Out.WriteLineAsync($"Received string: {System.Text.Encoding.UTF8.GetString(string_buffer)}");
            return System.Text.Encoding.UTF8.GetString(string_buffer);
        }

        public static async Task<string> ReceiveString(TcpClient client, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            byte[] string_buffer = await ReceiveVariable(client, token);
            await Console.Out.WriteLineAsync($"Received string: {System.Text.Encoding.UTF8.GetString(string_buffer)}");
            return System.Text.Encoding.UTF8.GetString(string_buffer);
        }

        public static async Task ReceiveFile(TcpClient client, Stream file, CancellationToken token, IProgress<double> progress = null)
        {
            token.ThrowIfCancellationRequested();

            long length = await ReceiveLong(client, token);

            await Console.Out.WriteLineAsync("Started receiving file");
            int pos = 0;
            byte[] buffer = new byte[1024];

            while (pos < length)
            {
                token.ThrowIfCancellationRequested();

                int read = await client.GetStream().ReadAsync(buffer, 0, (int)Math.Min(length - pos, buffer.Length), token);
                await file.WriteAsync(buffer, 0, read, token);
                pos += read;

                progress?.Report((double)pos / length * 100);
            }
            await Console.Out.WriteLineAsync($"Received {length} bytes");
        }
    }
    #endregion
}