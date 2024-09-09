using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HL7Server
{
    class Program
    {
        static void Main(string[] args)
        {
            StartHL7Server("127.0.0.1", 2575);
        }

        // Funktion zum Starten des HL7 v2 Servers
        public static void StartHL7Server(string ipAddress, int port)
        {
            // IP-Adresse und Port definieren
            IPAddress ip = IPAddress.Parse(ipAddress);
            TcpListener server = new TcpListener(ip, port);

            // Server starten
            server.Start();
            Console.WriteLine($"HL7 v2 Server läuft und wartet auf Verbindungen unter {ipAddress}:{port}...");

            while (true)
            {
                try
                {
                    // Auf eingehende Verbindung warten
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Verbindung akzeptiert.");

                    // Netzwerkstream für Kommunikation verwenden
                    NetworkStream stream = client.GetStream();

                    // Daten vom Client empfangen
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"Empfangene HL7-Nachricht: {receivedMessage}");

                    // HL7-Nachricht validieren
                    bool isValid;
                    string validationMessage = ValidateHL7Message(receivedMessage, out isValid);

                    // Antwort erstellen
                    string response;
                    if (isValid)
                    {
                        response = "ACK|AA|Message accepted\r";
                    }
                    else
                    {
                        response = $"NACK|AE|{validationMessage}\r";
                    }

                    // Antwort an den Client senden
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine($"Antwort gesendet: {response}");

                    // Verbindung schließen
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler: {ex.Message}");
                }
            }
        }

        // Funktion zur Validierung einer HL7 v2-Nachricht
        public static string ValidateHL7Message(string message, out bool isValid)
        {
            // Prüfen, ob die Nachricht mit "MSH" beginnt
            if (!message.StartsWith("MSH"))
            {
                isValid = false;
                return "Fehler: Nachricht enthält kein MSH-Segment.";
            }

            // Weitere Validierungen können hier hinzugefügt werden, z.B. Mindestanzahl an Feldern
            string[] segments = message.Split('\r');
            foreach (string segment in segments)
            {
                string[] fields = segment.Split('|');
                if (fields[0] == "MSH" && fields.Length < 9)
                {
                    isValid = false;
                    return "Fehler: MSH-Segment enthält nicht genügend Felder.";
                }
            }

            isValid = true;
            return "Nachricht ist gültig.";
        }
    }
}
