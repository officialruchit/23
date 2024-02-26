using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Security.Cryptography;
using System.Xml.Serialization;
namespace DeviceManager
{
    /// <summary>
    /// Read the xml file and 
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point of the Xml program.
        /// Parses command-line arguments, validates an XML file path, and manages device-related operations.
        /// </summary>
        /// <param name="args">Command-line arguments containing the XML file path.</param>
        static void Main(string[] args)
        {
            string xmlFilePath;

            if (args.Length == 1)
            {
                xmlFilePath = args[0];
            }
            else
            {
                Console.WriteLine($"Error: Invalid input. Program usage is as below.{Environment.NewLine}[DeviceUtil.exe] [XML file path]{Environment.NewLine}DeviceUtil.exe : Name of the executable file{Environment.NewLine}If anyone changes the name of the EXE, then the executable file name in usage should change accordingly.{Environment.NewLine}Terminate program.");
                return;
            }

            // XML file validation
            if (!File.Exists(xmlFilePath))
            {
                Console.WriteLine($"Error: File does not exist. Please provide a valid file path.{Environment.NewLine} Terminate program.");
                return;
            }

            // Xml path validation
            if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
            {
                Console.WriteLine($"Error: Given file is not an XML file. The file extension is wrong.{Environment.NewLine}Terminate program.");
                return;
            }

            // Validate XML format and parse devices
            Dictionary<string, Device> devicesDictionary;
            try
            {
                devicesDictionary = ParseXml(xmlFilePath);
                if (devicesDictionary.Count == 0)
                {
                    Console.WriteLine($"Error: No devices found in the XML file.{Environment.NewLine}Terminate program.");
                    return;
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine("Error: Invalid XML format. " + ex.Message);
                Console.WriteLine("Please check the XML file and fix the formatting issues.");
                Console.WriteLine("Terminate program.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: An unexpected error occurred while parsing XML. " + ex.Message);
                Console.WriteLine("Terminate program.");
                return;
            }


            while (true)
            {
                Console.WriteLine("Please select an option:");
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

        /// <summary>
        /// Parses an XML file containing device information and validates the content.
        /// </summary>
        /// <param name="filePath">The path to the XML file.</param>
        /// <returns>A dictionary containing devices with their serial numbers as keys.</returns>
        static Dictionary<string, Device> ParseXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceList));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                DeviceList deviceList = (DeviceList)serializer.Deserialize(fileStream);
                Dictionary<string, Device> devicesDictionary = deviceList.Devices.ToDictionary(device => device.SrNo);
                // Dictionary to keep track of serial numbers and their corresponding device index
                Dictionary<string, int> serialNumberIndexMap = new Dictionary<string, int>();

                // Dictionary to keep track of addresses and their corresponding device index
                Dictionary<string, int> addressIndexMap = new Dictionary<string, int>();
                // Validate devices
                int deviceIndex = 1;
                foreach (var device in deviceList.Devices)
                {
                    if (!IsValidDevice(device) )
                    {
                        if (serialNumberIndexMap.ContainsKey(device.SrNo))
                        {
                            Console.WriteLine($"Serial Number: {device.SrNo ?? "not present"} (duplicate)");
                        }
                        else
                        {
                            serialNumberIndexMap.Add(device.SrNo, deviceIndex);
                            Console.WriteLine($"Serial Number: {device.SrNo ?? "not present"} {(ValidateAndFormatString(device.SrNo, 16) ? "" : " (invalid length)")}  {(InvaliCharacter(device.SrNo) ? "" : " (invalid Character)")}");

                        }
                        if (addressIndexMap.ContainsKey(device.Address))
                        {
                            Console.WriteLine($"IP Address: {device.Address ?? "not present"} (duplicate ) {(ValidateAndFormatString(device.Address, 15) ? "" : "(invalid length)")}");
                        }
                        else
                        {
                            addressIndexMap.Add(device.Address, deviceIndex);
                            Console.WriteLine($"IP Address: {(string.IsNullOrEmpty(device.Address) ? "not present" : string.IsNullOrWhiteSpace(device.Address) ? "empty" : device.Address)} {(ValidateAndFormatString(device.Address, 15) ? "" : "(invalid length)")}{(ValidateAddressFormat(device.Address) ? "" : "(invalid formate)")}");
                        }
                   /*     Console.WriteLine($"IP Address: {(string.IsNullOrEmpty(device.Address) ? "not present" : string.IsNullOrWhiteSpace(device.Address) ? "empty" : device.Address)}");*/
                        Console.WriteLine($"Device Name: {device.DevName ?? "not present"} {(IsEmpty(device.DevName)?"":"(Empty)")}   {(InvaliCharacter(device.DevName) ? "" : " (invalid Character)")} {(ValidateAndFormatString(device.DevName, 24) ? "" : "(invalid length)")}");
                        Console.WriteLine($"Model Name: {device.ModelName ?? "not present"}{(IsEmpty(device.ModelName) ? "" : "(Empty)")}{(ValidateAndFormatString(device.ModelName, 24) ? "" : "(invalid length)")}   {(InvaliCharacter(device.ModelName) ? "" : " (invalid Character)")}");
                        Console.WriteLine($"Type: {device.Type ?? "not present"}{(ValidateTypeFormat(device.Type) ? "" : "(invalid formate)")}{(IsEmpty(device.Type) ? "" : "(Empty)")} ");
                        Console.WriteLine($"Port Number: {device.CommSetting.PortNo ?? "not present"}{(ValidatePortNumberFormat(device.CommSetting.PortNo) ? "" : "(invalid formate)")}{(IsEmpty(device.CommSetting.PortNo) ? "" : "(Empty)")} ");
                        Console.WriteLine($"Use SSL: {(string.IsNullOrEmpty(device.CommSetting.UseSSL) ? "not present" : device.CommSetting.UseSSL)} {(ValidateUseSSLFormat(device.CommSetting.UseSSL) ? "" : "(invalid formate)")}{(IsEmpty(device.CommSetting.UseSSL) ? "" : "(Empty)")} ");
                        Console.WriteLine($"Password: {(string.IsNullOrEmpty(device.CommSetting.Password) ? "not present" : device.CommSetting.Password)}  {(ValidateAndFormatString(device.CommSetting.Password, 24) ? "" : "(invalid length)")} {(IsEmpty(device.CommSetting.Password) ? "" : "(Empty)")} {(InvaliCharacter(device.CommSetting.Password) ? "" : " (invalid Character)")}");
                        Console.WriteLine();
                        deviceIndex++;
                        break;
                    }

                    deviceIndex++;
                }
                return devicesDictionary;
            }
        }


       /* static string DecryptPassword(string encryptedPassword)
        {
            // Implement your decryption logic here
            // Use the same key and IV that you used for encryption
            DataEncryptor keys = new DataEncryptor();
            return keys.DecryptString(encryptedPassword);
        }*/



        /// <summary>
        /// Validates if a string contains any invalid characters.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <returns>True if the input string is null or does not contain any of the predefined invalid characters; otherwise, false.</returns>
        public static bool InvaliCharacter(string input)
        {
            var invalidCharacters = new HashSet<char>("!@#$%^&*()-+=<>?/\\|[]{}");
            if (input != null && input.Any(c => invalidCharacters.Contains(c)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates the format of an address string.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <returns>True if the input string is not null or empty and conforms to the alphanumeric address format (A-Z, 0-9); otherwise, false.</returns>
        static bool ValidateAddressFormat(string input)
        {
            // Address format: alphanumeric (A-Z, 0-9)
            return !string.IsNullOrEmpty(input) && System.Text.RegularExpressions.Regex.IsMatch(input, "^[A-Za-z0-9.]*$");
        }

        /// <summary>
        /// Validates the format of a string representing a device type.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <returns>True if the input string is not null or empty and represents a valid device type format ("A3" or "A4"); otherwise, false.</returns>
        static bool ValidateTypeFormat(string input)
        {
            // Type format: A3 or A4
            return !string.IsNullOrEmpty(input) && (input == "A3" || input == "A4");
        }

        /// <summary>
        /// Validates the format of a string representing a port number.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <returns>True if the input string is not null or empty and consists of only digits (0-9); otherwise, false.</returns>
        static bool ValidatePortNumberFormat(string input)
        {
            // Port number format: digits (0-9)
            return !string.IsNullOrEmpty(input) && System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9]*$");
        }

        /// <summary>
        /// Validates the format of a string representing the use of SSL.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <returns>True if the input string is not null or empty and represents a valid SSL format ("true" or "false"); otherwise, false.</returns>
        static bool ValidateUseSSLFormat(string input)
        {
            // Use SSL format: true or false
            return !string.IsNullOrEmpty(input) && (input.ToLower() == "true" || input.ToLower() == "false");
        }

        /// <summary>
        /// Validates the length of a string against a specified maximum length.
        /// </summary>
        /// <param name="input">The input string to be validated.</param>
        /// <param name="maxLength">The maximum allowed length for the string.</param>
        /// <returns>True if the string is not null and its length does not exceed the specified maximum length; otherwise, false.</returns>
        static bool ValidateAndFormatString(string input, int maxLength)
        {
          

            if (input.Length > maxLength)
                return false;
            return true;
        }

        static bool IsEmpty(string input)
        {
            if (input.Length == 0) { 
            return false;
            }
            return true;
        }
        /// <summary>
        /// Validates if the provided device object meets the required criteria for a valid device.
        /// </summary>
        /// <param name="device">Device object to be validated.</param>
        /// <returns>True if the device is valid; otherwise, false.</returns>
        static bool IsValidDevice(Device device)
        {
            // Check if any of the required fields are empty or not present
            if (string.IsNullOrWhiteSpace(device.SrNo) ||
            string.IsNullOrWhiteSpace(device.Address) ||
            string.IsNullOrWhiteSpace(device.DevName) ||
             string.IsNullOrWhiteSpace(device.Type) ||
             device.CommSetting == null ||
             string.IsNullOrEmpty(device.CommSetting.PortNo) ||
             string.IsNullOrWhiteSpace(device.CommSetting.Password) ||
             string.IsNullOrWhiteSpace(device.CommSetting.UseSSL)) // Fix logical issue here
            {
                return false;
            }

            if (InvaliCharacter(device.SrNo)==false)
            {
                return false;
            }

            if (InvaliCharacter(device.Address)==false)
            {
                return false;
            }

            if (InvaliCharacter(device.ModelName) == false)
            {
                return false;
            }

            if (InvaliCharacter(device.DevName) == false)
            {
                return false;
            }

            if (InvaliCharacter(device.CommSetting.Password)==false)
            {
                return false;
            }

            if (ValidateAddressFormat(device.DevName)==false)
            {
                return false;
            }

            if (ValidateAddressFormat(device.CommSetting.PortNo) == false)
            {
                return false;
            }

            if (ValidateAddressFormat(device.Type) == false)
            {
                return false;
            }

            if (ValidateAddressFormat(device.CommSetting.UseSSL) == false)
            {
                return false;
            }

            if ((ValidateAndFormatString(device.ModelName, 24))==false)
            {
                return false;
            }

            if ((ValidateAndFormatString(device.SrNo, 16)))
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
            if (IsValidIPAddress(device.Address)) 
            { 
            return false;   
            }
            if (IsEmpty(device.DevName)) { 
            return false;
            }
            // Additional validation checks can be added her

            return true;
        }

        /// <summary>
        /// Validate the formate of the IPAdress
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        static bool IsValidIPAddress(string ipAddress)
        {
            // Basic validation for IPv4 address format
            return IPAddress.TryParse(ipAddress, out _);
        }

        /// <summary>
        /// Displays a formatted table presenting information about devices stored in a dictionary.
        /// </summary>
        /// <param name="devices">Dictionary containing devices with serial numbers as keys.</param>
        static void ShowDevices(Dictionary<string, Device> devices)
        {
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", "No", "Serial Number", "IP Address", "Device Name", "Model Name", "Type", "Port", "SSL", "Password");
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            int i = 1;
            foreach (var device in devices.Values)
            {
                Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", i++, device.SrNo, device.Address, device.DevName, device.ModelName, device.Type, device.CommSetting.PortNo, device.CommSetting.UseSSL, device.CommSetting.Password);
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
        }

        /// <summary>
        /// Searches for a device in the provided dictionary by its serial number and displays its information.
        /// </summary>
        /// <param name="devices">Dictionary containing devices with serial numbers as keys.</param>
        /// <param name="serialNumber">Serial number of the device to search for.</param>
        static void SearchDevice(Dictionary<string, Device> devices, string serialNumber)
        {
            if (devices.ContainsKey(serialNumber))
            {
                Device device = devices[serialNumber];
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-10} {5,-10} {6,-10}", "Serial Number", "IP Address", "Device Name", "Model Name", "Type", "Port", "SSL", "Password");
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("{0,-20} {1,-20} {2,-20} {3,-20} {4,-10} {5,-10} {6,-10}", device.SrNo, device.Address, device.DevName, device.ModelName, device.Type, device.CommSetting.PortNo, device.CommSetting.UseSSL, device.CommSetting.Password);
                Console.WriteLine("-------------------------------------------------------------------------------------------------------------------------------------------");
            }
            else
            {
                Console.WriteLine("Device not found.");
            }
        }
    }
}  
