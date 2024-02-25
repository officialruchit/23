using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;

namespace DeviceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string xmlFilePath;

            if (args.Length == 1)
            {
                xmlFilePath = args[0];
            }
            else
            {
                Console.WriteLine("Error: Invalid input. Program usage is as below.");
                Console.WriteLine("[DeviceUtil.exe] [XML file path]");
                Console.WriteLine("DeviceUtil.exe : Name of the executable file");
                Console.WriteLine("If anyone changes the name of the EXE, then the executable file name in usage should change accordingly.");
                Console.WriteLine("Terminate program.");
                return;
            }

            // XML file validation
            if (!File.Exists(xmlFilePath))
            {
                Console.WriteLine("Error: File does not exist. Please provide a valid file path.");
                Console.WriteLine("Terminate program.");
                return;
            }

            if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
            {
                Console.WriteLine("Error: Given file is not an XML file. The file extension is wrong.");
                Console.WriteLine("Terminate program.");
                return;
            }

            // Validate XML format and parse devices
            Dictionary<string, Device> devicesDictionary;
            try
            {
                devicesDictionary = ParseXml(xmlFilePath);

                if (devicesDictionary.Count == 0)
                {
                    Console.WriteLine("Error: No devices found in the XML file.");
                    Console.WriteLine("Terminate program.");
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: An unexpected error occurred while parsing XML. " + ex.Message);
                Console.WriteLine("Terminate program.");
                return;
            }


            while (true)
            {
                Console.WriteLine("\nPlease select an option:");
                Console.WriteLine("[1] Show all devices");
                Console.WriteLine("[2] Search devices by serial number");
                Console.WriteLine("[3] Exit");

                string choice = Console.ReadLine().Trim();

                switch (choice)
                {
                    case "1":
                        ShowDevices(devicesDictionary);
                        break;
                    case "2":
                        Console.Write("Enter serial number of the device: ");
                        string serialNumber = Console.ReadLine().Trim();
                        SearchDevice(devicesDictionary, serialNumber);
                        break;
                    case "3":
                        Console.WriteLine("Program terminated.");
                        return;
                    default:
                        Console.WriteLine("Error: Invalid input. Please choose from the above options.");
                        break;
                }
            }
        }

        static Dictionary<string, Device> ParseXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceList));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                DeviceList deviceList = (DeviceList)serializer.Deserialize(fileStream);
                Dictionary<string, Device> devicesDictionary = deviceList.Devices.ToDictionary(device => device.SerialNumber);


                // Dictionary to keep track of serial numbers and their corresponding device index
                Dictionary<string, int> serialNumberIndexMap = new Dictionary<string, int>();

                // Dictionary to keep track of addresses and their corresponding device index
                Dictionary<string, int> addressIndexMap = new Dictionary<string, int>();
                // Validate devices
                int deviceIndex = 1;
                foreach (var device in deviceList.Devices)
                {



                    if (!IsValidDevice(device)|| ValidateAddressFormat(device.DevName)==false)
                    {
                        if (serialNumberIndexMap.ContainsKey(device.SerialNumber))
                        {
                            Console.WriteLine($"Serial Number: {device.SerialNumber ?? "not present"} (duplicate)");
                        }
                        else
                        {
                            serialNumberIndexMap.Add(device.SerialNumber, deviceIndex);
                            Console.WriteLine($"Serial Number: {device.SerialNumber ?? "not present"} {(ValidateAndFormatString(device.SerialNumber, 16) ? "" : " (invalid length)")}  {(InvaliCharacter(device.SerialNumber) ? "" : " (invalid Character)")}   ");

                        }
                        if (addressIndexMap.ContainsKey(device.Address))
                        {
                            Console.WriteLine($"IP Address: {device.Address ?? "not present"} (duplicate )");
                        }
                        else
                        {
                            addressIndexMap.Add(device.Address, deviceIndex);
                            Console.WriteLine($"IP Address: {(string.IsNullOrEmpty(device.Address) ? "not present" : string.IsNullOrWhiteSpace(device.Address) ? "empty" : device.Address)} {(ValidateAddressFormat(device.Address) ? "" : "(invalid formate)")}");
                        }
                        Console.WriteLine($"IP Address: {(string.IsNullOrEmpty(device.Address) ? "not present" : string.IsNullOrWhiteSpace(device.Address) ? "empty" : device.Address)}");
                      
                        Console.WriteLine($"Device Name: {device.DevName ?? "not present"}    {(InvaliCharacter(device.DevName) ? "" : " (invalid Character)")} {(ValidateAndFormatString(device.DevName, 24) ? "" : "(invalid length)")}");
                        Console.WriteLine($"Model Name: {device.ModelName ?? "not present"}{(ValidateAndFormatString(device.ModelName, 24) ? "" : "(invalid length)")}   {(InvaliCharacter(device.ModelName) ? "" : " (invalid Character)")}");
                        Console.WriteLine($"Type: {device.Type ?? "not present"}{(ValidateTypeFormat(device.Type) ? "" : "(invalid formate)")}");
                        Console.WriteLine($"Port Number: {device.CommSetting.PortNo ?? "not present"}{(ValidatePortNumberFormat(device.CommSetting.PortNo) ? "" : "(invalid formate)")}");
                        Console.WriteLine($"Use SSL: {(string.IsNullOrEmpty(device.CommSetting.UseSSL) ? "not present" : device.CommSetting.UseSSL)} {(ValidateUseSSLFormat(device.CommSetting.UseSSL) ? "" : "(invalid formate)")}");
                        Console.WriteLine($"Password: {(string.IsNullOrEmpty(device.CommSetting.Password) ? "not present" : device.CommSetting.Password)}  {(ValidateAndFormatString(device.CommSetting.Password, 24) ? "" : "(invalid length)")} {(InvaliCharacter(device.CommSetting.Password) ? "" : " (invalid Character)")}");
                        Console.WriteLine();
                        deviceIndex++;
                    }

                    deviceIndex++;
                }

                return devicesDictionary;
            }
        }





        public static bool InvaliCharacter(string input)
        {

            var invalidCharacters = new HashSet<char>("!@#$%^&*()-+=<>?/\\|[]{}");
            if (input != null && input.Any(c => invalidCharacters.Contains(c)))
            {
                return false;
            }
            return true;
        }



        static bool ValidateAddressFormat(string input)
        {
            // Address format: alphanumeric (A-Z, 0-9)
            return !string.IsNullOrEmpty(input) && System.Text.RegularExpressions.Regex.IsMatch(input, "^[A-Za-z0-9.]*$");
        }

        static bool ValidateTypeFormat(string input)
        {
            // Type format: A3 or A4
            return !string.IsNullOrEmpty(input) && (input == "A3" || input == "A4");
        }

        static bool ValidatePortNumberFormat(string input)
        {
            // Port number format: digits (0-9)
            return !string.IsNullOrEmpty(input) && System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9]*$");
        }

        static bool ValidateUseSSLFormat(string input)
        {
            // Use SSL format: true or false
            return !string.IsNullOrEmpty(input) && (input.ToLower() == "true" || input.ToLower() == "false");
        }


        static bool ValidateAndFormatString(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            if (input.Length > maxLength)
                return false;
           
            

            return true;
        }

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        static bool IsValidDevice(Device device)
        {
            // Check if any of the required fields are empty or not present
            if (string.IsNullOrWhiteSpace(device.SerialNumber) ||
                string.IsNullOrWhiteSpace(device.Address) ||
                string.IsNullOrWhiteSpace(device.DevName) ||
                string.IsNullOrWhiteSpace(device.Type) ||
                device.CommSetting == null  // Check if CommSetting node is not present
                /*device.CommSetting.PortNo == 0 */|| // Assuming port number cannot be 0 if provided
                string.IsNullOrWhiteSpace(device.CommSetting.Password))
            {
                return false;
            }


            if (!InvaliCharacter(device.SerialNumber))
            {
                return false;
            }

            if (!InvaliCharacter(device.Address))
            {
                return false;
            }

            if (!InvaliCharacter(device.ModelName))
            {
                return false;
            }

            if (!InvaliCharacter(device.DevName))
            {
                return false;
            }

            if (!InvaliCharacter(device.CommSetting.Password))
            {
                return false;
            }




            if (!ValidateAddressFormat(device.DevName)) {
                return false;
            }
            if (!ValidateAddressFormat(device.CommSetting.PortNo))
            {
                return false;
            }
            if (!ValidateAddressFormat(device.Type))
            {
                return false;
            }
            if (!ValidateAddressFormat(device.CommSetting.UseSSL))
            {
                return false;
            }


            if ((ValidateAndFormatString(device.ModelName, 24)))
                {
                return false;
            }
            if ((ValidateAndFormatString(device.SerialNumber, 16)))
            {
                return false;
            }
            if ((ValidateAndFormatString(device.Address, 15)))
            {
                return false;
            }
            if ((ValidateAndFormatString(device.DevName, 24)))
            {
                return false;
            }
            if ((ValidateAndFormatString(device.CommSetting.Password, 64)))
            {
                return false;
            }

            // Additional validation checks can be added here

            return true;
        }

        static bool IsValidIPAddress(string ipAddress)
        {
            // Basic validation for IPv4 address format
            return IPAddress.TryParse(ipAddress, out _);
        }

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        static void ShowDevices(Dictionary<string, Device> devices)
        {
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", "No", "Serial Number", "IP Address", "Device Name", "Model Name", "Type", "Port", "SSL", "Password");
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            int i = 1;
            foreach (var device in devices.Values)
            {
                Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", i++, device.SerialNumber, device.Address, device.DevName, device.ModelName, device.Type, device.CommSetting.PortNo, device.CommSetting.UseSSL, device.CommSetting.Password);
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
        }

   
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        static void SearchDevice(Dictionary<string, Device> devices, string serialNumber)
        {
            if (devices.ContainsKey(serialNumber))
            {
                Device device = devices[serialNumber];
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-10} {5,-10} {6,-10}", "Serial Number", "IP Address", "Device Name", "Model Name", "Type", "Port", "SSL", "Password");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-10} {5,-10} {6,-10}", device.SerialNumber, device.Address, device.DevName, device.ModelName, device.Type, device.CommSetting.PortNo, device.CommSetting.UseSSL, device.CommSetting.Password);
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Device not found.");
            }
        }
    }
}
