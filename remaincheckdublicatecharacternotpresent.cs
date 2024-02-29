

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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


            /*
                        if (!ValidateXml(xmlFilePath))
                        {
                            Console.WriteLine("Error: XML file does not conform to the schema. Terminate program.");
                            return;
                        }*/
            // Convert the list of devices to a dictionary for easier access
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


        /*  static bool ValidateXml(string filePath)
          {
              XmlSchemaSet schemaSet = new XmlSchemaSet();
              schemaSet.Add("", "C:\\Users\\Lenovo\\source\\repos\\DeserialXml\\DeserialXml\\xsd.xsd"); // Replace "DeviceSchema.xsd" with the path to your XSD schema file

              XmlReaderSettings settings = new XmlReaderSettings();
              settings.ValidationType = ValidationType.Schema;
              settings.Schemas = schemaSet;
              settings.ValidationEventHandler += ValidationEventHandler;

              try
              {
                  using (XmlReader reader = XmlReader.Create(filePath, settings))
                  {
                      while (reader.Read()) ;
                  }

                  return true; // XML validation passed
              }
              catch (Exception ex)
              {
                  Console.WriteLine($"Error: An error occurred while validating XML against the schema. {ex.Message}");
                  return false; // XML validation failed
              }
          }*/

        /*       static void ValidationEventHandler(object sender, ValidationEventArgs e)
               {
                   if (e.Severity == XmlSeverityType.Error)
                   {
                       // Custom error messages for specific validation errors
                       switch (e.Exception.Message)
                       {
                           case "The 'Address' element is invalid - The value 'INVALID_ADDRESS' is invalid according to its datatype 'http://www.w3.org/2001/XMLSchema:string' - The Pattern constraint failed.":
                               Console.WriteLine("Error: Invalid address format.");
                               break;
                           case "The 'UseSSL' element is invalid - The value 'INVALID_USESSL' is invalid according to its datatype 'http://www.w3.org/2001/XMLSchema:boolean' - The string 'INVALID_USESSL' is not a valid Boolean value.":
                               Console.WriteLine("Error: Invalid UseSSL format.");
                               break;
                           case "The 'Password' element is invalid - The value 'INVALID_PASSWORD' is invalid according to its datatype 'http://www.w3.org/2001/XMLSchema:string' - The Length constraint failed.":
                               Console.WriteLine("Error: Invalid password format.");
                               break;
                           case "The 'Type' element is invalid - The value 'INVALID_TYPE' is invalid according to its datatype 'http://www.w3.org/2001/XMLSchema:string' - The Enumeration constraint failed.":
                               Console.WriteLine("Error: Invalid Type format.");
                               break;
                           default:
                               Console.WriteLine($"Error: {e.Exception.Message}");
                               break;
                       }
                   }
               }*/




        static Dictionary<string, Device> ParseXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceList));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                DeviceList deviceList = (DeviceList)serializer.Deserialize(fileStream);
                Dictionary<string, Device> devicesDictionary = deviceList.Devices.ToDictionary(device => device.SrNo);



                // Validate devices
                int deviceIndex = 1;
                foreach (var device in deviceList.Devices)
                {



                    if (!IsValidDevice(device))
                    {
                        Console.WriteLine("Error: Invalid device information. Please refer below details.");
                        Console.WriteLine($"Device index: {deviceIndex}");
                        Console.WriteLine($"Serial Number: {(InvalideLength(device.SrNo, 16) ? "" : "(invalid length)")} {(IsEmpty(device.SrNo) ? "" : "(Empty)")}");
                        Console.WriteLine($"IP Address: {(InvalideLength(device.Address, 15) ? "" : "(invalid length)")} {(IsEmpty(device.Address) ? "" : "(Empty)")} {(ValidateAddressFormat(device.Address) ? "" : "(Not Supported formate)")}");
                        Console.WriteLine($"Device Name:{(InvalideLength(device.DevName, 24) ? "" : "(invalid length)")} ");
                        Console.WriteLine($"Model Name: {(InvalideLength(device.ModelName, 24) ? "" : "(invalid length)")}");
                        Console.WriteLine($"Type: {(IsEmpty(device.Type) ? "" : "(Empty)")} {(ValidateTypeFormat(device.Type) ? "" : "(Not Supported formate)")}");
                        Console.WriteLine($"Port Number: {(IsEmpty(device.CommSetting.PortNo) ? "" : "(Empty)")} {(ValidatePortNumberFormat(device.CommSetting.PortNo) ? "" : "(Not Supported formate)")}"); // Assuming port number cannot be 0 if provided
                        Console.WriteLine($"Use SSL: {(IsEmpty(device.CommSetting.UseSSL) ? "" : "(Empty)")} {(ValidateUseSSLFormat(device.CommSetting.UseSSL) ? "" : "(Not Supported formate)")}"); // Assuming UseSSL is a boolean
                        Console.WriteLine($"Password: {(InvalideLength(device.CommSetting.Password, 64) ? "" : "(invalid length)")} {(IsEmpty(device.CommSetting.Password) ? "" : "(Empty)")} {(invalideCharacter(device.CommSetting.Password) ? "" : "(not supported character)")}");
                        Console.WriteLine();
                    }

                    deviceIndex++;
                }

                return devicesDictionary;
            }
        }

        static bool invalideCharacter(string input)
        {
            var pattern = @"^[a-zA-Z0-9]";
            return Regex.IsMatch(input, pattern);

        }

        static bool ValidatePortNumberFormat(string input)
        {
            // Port number format: digits (0-9)
            return System.Text.RegularExpressions.Regex.IsMatch(input, "^[0-9]*$");
        }

        static bool ValidateTypeFormat(string input)
        {
            // Type format: A3 or A4
            return (input == "A3" || input == "A4");
        }
        static bool ValidateAddressFormat(string input)
        {
            // Address format: alphanumeric (A-Z, 0-9)
            return !string.IsNullOrEmpty(input) && System.Text.RegularExpressions.Regex.IsMatch(input, "^[A-Za-z0-9.]*$");
        }

        static bool ValidateUseSSLFormat(string input)
        {
            // Use SSL format: true or false
            return !string.IsNullOrEmpty(input) && (input.ToLower() == "true" || input.ToLower() == "false");
        }
        static bool IsEmpty(string input)
        {
            if (input.Length == 0)
            {
                return false;
            }
            return true;
        }

        static bool InvalideLength(string input, int maxLength)
        {
            if (input.Length > maxLength)
                return false;
            return true;
        }


        static bool IsValidDevice(Device device)
        {
            // Check if any of the required fields are empty or not present
            if (string.IsNullOrWhiteSpace(device.SrNo) ||
                string.IsNullOrWhiteSpace(device.Address) ||
                string.IsNullOrWhiteSpace(device.DevName) ||
                string.IsNullOrWhiteSpace(device.Type) ||
                device.CommSetting == null  // Check if CommSetting node is not present
                /*device.CommSetting.PortNo == 0 */|| // Assuming port number cannot be 0 if provided
                string.IsNullOrWhiteSpace(device.CommSetting.Password))
            {
                return false;
            }

            // Additional validation checks can be added h


            if ((!InvalideLength(device.SrNo, 16)))
            {
                return false;
            }

            if ((!InvalideLength(device.Address, 15)))
            {
                return false;
            }

            if ((!InvalideLength(device.DevName, 24)))
            {
                return false;
            }

            if ((!InvalideLength(device.ModelName, 24)))
            {
                return false;
            }

            if ((!InvalideLength(device.CommSetting.Password, 64)))
            {
                return false;
            }
            if (!IsEmpty(device.CommSetting.Password))
            {
                return false;
            }
            if (!ValidateTypeFormat(device.Type))
            {
                return false;
            }
            if (!ValidatePortNumberFormat(device.CommSetting.PortNo))
            {
                return false;
            }
            if (!invalideCharacter(device.CommSetting.Password))
            {
                return false;
            }

            /*
                        if (ValidateUseSSLFormat(device.CommSetting.UseSSL))
                        {
                            return false;
                        }*/


            /* if (!ValidateAddressFormat(device.Address))
             {
                 return false;
             }*/


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
                Console.WriteLine("{0,-5} {1,-20} {2,-20} {3,-20} {4,-20} {5,-10} {6,-10} {7,-10}", i++, device.SrNo, device.Address, device.DevName, device.ModelName, device.Type, device.CommSetting.PortNo, device.CommSetting.UseSSL, device.CommSetting.Password);
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
