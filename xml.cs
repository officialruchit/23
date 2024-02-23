using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace DeviceManager
{
    /// <summary>
    /// presented program show the validate xml and provide searching functioonality
    /// </summary>
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
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
                Console.ReadKey();
                return;
            }


            if (File.Exists(xmlFilePath) == false)
            {
                Console.WriteLine($"Error: File does not exist. Please provide a valid file path. {Environment.NewLine}Terminate program. pathh bro ");
                return;
            }

            if (Path.GetExtension(xmlFilePath).ToLower() != ".xml")
            {
                Console.WriteLine($"Error: Given file is not an XML file. The file extension is wrong. {Environment.NewLine}Terminate program.");
                return;
            }


            Dictionary<string, Device> devicesDictionary;
            try
            {
                devicesDictionary = ParseXml(xmlFilePath);
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
                Console.ReadLine();
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static Dictionary<string, Device> ParseXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DeviceList));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                DeviceList deviceList = (DeviceList)serializer.Deserialize(fileStream);
                Dictionary<string, Device> devicesDictionary = deviceList.Devices.ToDictionary(device => device.SerialNumber);


                int deviceIndex = 1;

                if (deviceList.Devices == null || deviceList.Devices.Count == 0)
                {
                    Console.WriteLine("Error: The XML file is empty. Device data is not present in the file.");
                    Environment.Exit(0);

                }

                foreach (var device in deviceList.Devices)
                {

                    if (!IsValidDevice(device) ||
                        IsValidString(device.SerialNumber) == false
                        || formate(device.CommSetting.UseSSL) == false
                        || formate(device.Type) == false

                        || InvaliCharacter(device.ModelName) == false
                        || InvaliCharacter(device.SerialNumber) == false
                        || InvaliCharacter(device.Address) == false
                        || InvaliCharacter(device.DevName) == false
                        || InvaliCharacter(device.CommSetting.Password) == false
                        )
                    {
                        Console.WriteLine("Error: Invalid device information. Please refer below details.");
                        Console.WriteLine($"Device index: {deviceIndex}");
                        Console.WriteLine($"Serial Number: {device.SerialNumber ?? "(Not present)"}{(InvaliCharacter(device.SerialNumber) ? "" : "(Invalid character)")} {(IsValidLength(device.SerialNumber, 16, 16) ? "" : "(Invalid length)")} ");
                        Console.WriteLine($"IP Address: {device.Address ?? "(Not present)"} {(InvaliCharacter(device.Address) ? "" : "(Invalid character)")} {(IsValidIPAddress(device.Address) ? "" : "(Not supported format)")} {(IsValidLength(device.Address, 1, 15) ? "" : "(Invalid length)")}");

                        Console.WriteLine($"Device Name: {device.DevName ?? "(Not present)"}{(InvaliCharacter(device.DevName) ? "" : "(Invalid character)")} {(IsValidLength(device.DevName, 0, 24) ? "" : "(Invalid length)")}");
                        Console.WriteLine($"Model Name:   {(InvaliCharacter(device.ModelName) ? "" : "(Invalid character)")}  {(IsValidLength(device.ModelName, 0, 24) ? "" : "(Invalid length)")}");
                        Console.WriteLine($"Type: {device.Type ?? "(Not present)"}{(formate(device.Type) ? "" : "(Invalid formate)")}");
                        Console.WriteLine($"Port Number: {device.CommSetting.PortNo ?? "(Not present)"}");
                        Console.WriteLine($"Use SSL: {device.CommSetting.UseSSL ?? "(Not present)"}");
                        Console.WriteLine($"Password: {device.CommSetting.Password ?? "(Not present)"}{(InvaliCharacter(device.CommSetting.Password) ? "" : "(Invalid character)")} {(IsValidLength(device.CommSetting.Password, 0, 64) ? "" : "(Invalid length)")} ");
                        Console.WriteLine();
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        static bool IsValidDevice(Device device)
        {
            if (string.IsNullOrWhiteSpace(device.Address) ||

                string.IsNullOrWhiteSpace(device.DevName) ||
                string.IsNullOrWhiteSpace(device.Type) ||
                string.IsNullOrWhiteSpace(device.CommSetting.PortNo) ||
                string.IsNullOrWhiteSpace(device.CommSetting.Password) ||
                string.IsNullOrWhiteSpace(device.CommSetting.UseSSL) ||
                string.IsNullOrWhiteSpace(device.CommSetting.PortNo))

            {
                return false;
            }

            if (!IsValidString(device.SerialNumber) ||
                !IsValidString(device.DevName) ||
                !IsValidString(device.CommSetting.Password))
            {

                return false;
            }
            if (IsValidIPAddress(device.Address) == false)
            {
                return false;
            }

            if (!IsValidLength(device.SerialNumber, 16, 16) ||
          !IsValidLength(device.Address, 1, 15) ||
          !IsValidLength(device.DevName, 0, 24) ||
          !IsValidLength(device.ModelName, 0, 24) ||
          !IsValidLength(device.CommSetting.Password, 0, 64))
            {
                return false;
            }
            if (!IsValidString(device.SerialNumber))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static bool IsValidString(string input)
        {

            return System.Text.RegularExpressions.Regex.IsMatch(input, "^[A-Z0-9]+$");
        }
        /*
                public static bool InvalideCharacter(string input)
                {
                    if(System.Text.RegularExpressions.Regex.IsMatch(input,))

                    return true;

                }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static bool formate(string input)
        {

            if ((input == "true" || input == "false") == false)
            {
                return false;
            }
            if ((input != "A4" || input != "A3"))
            {
                return false;
            }
            if (int.TryParse(input, out int input1) && input1 < 0 || input1 > 9)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        static bool IsValidLength(string value, int minLength, int maxLength)
        {
            int length = value.Length;
            return length >= minLength && length <= maxLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        static bool IsValidIPAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="devices"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="serialNumber"></param>
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




/*
static bool ValidateDeviceInformation(string serialNumber, string modelName, string deviceName, string ipAddress, string password)
{
    bool isValid = true;

    // Validate SerialNumber
    if (!ValidateSerialNumber(serialNumber))
    {
        DisplayErrorMessage("Serial Number", serialNumber, "Invalid character");
        isValid = false;
    }

    // Validate ModelName
    if (!ValidateModelName(modelName))
    {
        DisplayErrorMessage("Model Name", modelName, "Invalid character");
        isValid = false;
    }

    // Validate DeviceName
    if (!ValidateDeviceName(deviceName))
    {
        DisplayErrorMessage("Device Name", deviceName, "Invalid character");
        isValid = false;
    }

    // Validate IPAddress
    if (!ValidateIPAddress(ipAddress))
    {
        DisplayErrorMessage("IP Address", "Not present", "Invalid format");
        isValid = false;
    }

    // Validate Password
    if (!ValidatePassword(password))
    {
        DisplayErrorMessage("Password", "Not present", "Invalid character");
        isValid = false;
    }

    return isValid;
}

static void DisplayErrorMessage(string fieldName, string value, string errorReason)
{
    Console.WriteLine($"Error: Invalid device information. Please refer below details.");
    Console.WriteLine($"{fieldName}: {value} ({errorReason})");
}*/
