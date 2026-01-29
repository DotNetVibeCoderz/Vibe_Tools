using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkScanner
{
    public class WakeOnLan
    {
        // Mengirim Magic Packet untuk membangunkan PC
        public static async Task SendMagicPacketAsync(string macAddress)
        {
            try
            {
                byte[] magicPacket = CreateMagicPacket(macAddress);
                using (UdpClient client = new UdpClient())
                {
                    client.EnableBroadcast = true;
                    // Kirim ke broadcast address port 9 (port standar WOL)
                    await client.SendAsync(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Broadcast, 9));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send WOL packet: {ex.Message}");
            }
        }

        private static byte[] CreateMagicPacket(string macAddress)
        {
            // Membersihkan format MAC Address
            macAddress = macAddress.Replace(":", "").Replace("-", "");

            if (macAddress.Length != 12)
                throw new ArgumentException("Invalid MAC Address format.");

            byte[] macBytes = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                macBytes[i] = byte.Parse(macAddress.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            // Struktur Magic Packet: 6 byte 0xFF, diikuti 16 kali MAC Address target
            byte[] packet = new byte[6 + 16 * 6];

            // 6 byte pertama 0xFF
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;

            // 16 kali MAC Address
            for (int i = 0; i < 16; i++)
            {
                Array.Copy(macBytes, 0, packet, 6 + i * 6, 6);
            }

            return packet;
        }
    }
}
