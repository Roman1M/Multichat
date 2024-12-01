using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace UDPClientApp
{
    public partial class MainWindow : Window
    {
        private UdpClient udpClient;
        private IPEndPoint serverEndPoint;
        private string userName;

        public MainWindow()
        {
            InitializeComponent();

            // Ініціалізація UDP клієнта
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Loopback, 3300); // Використовуємо локальний сервер (127.0.0.1) та порт 3300

            // Прив'язуємо сокет до порту (локальний порт)
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Loopback, 0)); // 0 означає автоматичний вибір порту

            // Запуск отримання повідомлень у окремому потоці
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        // Метод для підключення до сервера
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            userName = NameTextBox.Text; // Отримуємо ім'я користувача
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("Please enter your name.");
                return;
            }

            // Відправка повідомлення про підключення
            SendMessage($"{userName} has joined the chat.");

            // Відображення підключення
            ChatBox.Text += $"You are now connected as {userName}.\n";
            ConnectButton.IsEnabled = false; // Вимикаємо кнопку підключення
        }

        // Метод для відправки повідомлення на сервер
        private void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, serverEndPoint);
        }

        // Метод для отримання повідомлень від сервера
        private void ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    // Асинхронно отримуємо дані
                    UdpReceiveResult result = udpClient.ReceiveAsync().Result;
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

                    Dispatcher.Invoke(() =>
                    {
                        // Якщо сервер повідомляє, що він заповнений
                        if (receivedMessage == "Server is full. You cannot join the chat.")
                        {
                            MessageBox.Show("Server is full. You cannot join the chat.");
                            udpClient.Close(); // Закриваємо UDP з'єднання
                            this.Close(); // Закриваємо програму
                        }
                        else
                        {
                            // Оновлюємо інтерфейс користувача з отриманим повідомленням
                            ChatBox.Text += receivedMessage + "\n";
                        }
                    });
                }
                catch (Exception ex)
                {
                    // Якщо виникає помилка в потоці, вивести повідомлення
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                }
            }
        }

        // Обробник натискання кнопки Send
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (string.IsNullOrEmpty(message))
                return;

            // Додаємо ім'я, час і текст повідомлення
            string time = DateTime.Now.ToString("HH:mm:ss");
            string fullMessage = $"{userName} ({time}): {message}";
            SendMessage(fullMessage);

            // Додаємо повідомлення в діалог
            ChatBox.Text += $"{fullMessage}\n";
            MessageTextBox.Clear(); // Очищаємо поле вводу
        }

        // Обробник натискання кнопки Exit
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage($"{userName} has left the chat.");
            udpClient.Close(); // Закриваємо UDP клієнт
            this.Close(); // Закриваємо вікно
        }
    }
}
