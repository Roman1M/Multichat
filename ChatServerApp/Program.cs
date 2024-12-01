using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UDPServerApp
{
    class Program
    {
        private static UdpClient udpServer;
        private static List<IPEndPoint> clients;
        private static int maxClients = 5;  // Максимальна кількість клієнтів

        static async Task Main(string[] args)
        {
            udpServer = new UdpClient(3300); // Порт сервера
            clients = new List<IPEndPoint>(); // Список підключених клієнтів

            Console.WriteLine("Server is listening...");

            while (true)
            {
                // Асинхронно отримуємо дані від клієнта
                UdpReceiveResult result = await udpServer.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                IPEndPoint clientEndPoint = result.RemoteEndPoint;

                // Перевірка, чи досягнуто максимального числа клієнтів
                if (clients.Count >= maxClients)
                {
                    string fullMessage = "Server is full. You cannot join the chat.";
                    byte[] responseData = Encoding.UTF8.GetBytes(fullMessage);
                    await udpServer.SendAsync(responseData, responseData.Length, clientEndPoint); // Повідомляємо, що сервер заповнений
                    continue; // Не додаємо клієнта до списку
                }

                // Додаємо клієнта до списку, якщо його немає
                if (!clients.Contains(clientEndPoint))
                {
                    clients.Add(clientEndPoint);
                    string welcomeMessage = "You have successfully joined the chat!";
                    byte[] welcomeData = Encoding.UTF8.GetBytes(welcomeMessage);
                    await udpServer.SendAsync(welcomeData, welcomeData.Length, clientEndPoint); // Відправляємо повідомлення про підключення
                    Console.WriteLine($"Client connected: {clientEndPoint}");
                }

                Console.WriteLine($"Received: {receivedMessage}");

                // Відправка повідомлення всім клієнтам
                foreach (var client in clients)
                {
                    byte[] responseData = Encoding.UTF8.GetBytes(receivedMessage);
                    await udpServer.SendAsync(responseData, responseData.Length, client); // Відправляємо повідомлення
                    Console.WriteLine($"Sent to {client}: {receivedMessage}");
                }
            }
        }
    }
}
