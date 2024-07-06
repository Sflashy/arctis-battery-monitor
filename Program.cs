using HidSharp;

internal class Program
{
    
    enum DeviceStatus 
    {
        Offline,
        Online
    }

    /**
     * Gets the HidDevice with the specified device path.
     *
     * @param devicePath The path of the device.
     * @return The HidDevice with the specified device path, or null if not found.
     */
    private static HidDevice? GetDevice(string devicePath)
    {
        return DeviceList.Local.GetHidDevices().ToArray().Where(device => device.DevicePath.Contains(devicePath)).FirstOrDefault();
    }

    /// <summary>
    /// Main program entry point.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private static async Task Main(string[] args)
    {
        HidDevice? arctis = GetDevice("vid_1038&pid_12b3&mi_03&col02");
        if (arctis is null)
        {
            Console.WriteLine("Could not find the device");
            return;
        };
        bool connected = arctis.TryOpen(out HidStream? stream);
        if (connected)
        {
            await MonitorDeviceAsync(arctis, stream);
        }
        else
        {
            Console.WriteLine("Could not connect to the device");

        }
    }

    /// <summary>
    /// Monitors the device for battery status updates.
    /// </summary>
    /// <param name="arctis">The HID device.</param>
    /// <param name="stream">The HID stream.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private static async Task MonitorDeviceAsync(HidDevice arctis, HidStream stream)
    {
        byte[] data = { 0x06, 0x12 };
        byte[] responseBuffer = new byte[8];
        while (true)
        {
            await stream.WriteAsync(data);
            int bytesRead = await stream.ReadAsync(responseBuffer);
            string hexOfBytes = string.Join(" ", responseBuffer.Take(bytesRead).Select(b => b.ToString("X2")));
            string[] hexOfBytesArray = hexOfBytes.Split(" ");
            DeviceStatus status = Convert.ToInt32(hexOfBytesArray[2], 16) == 3 ? DeviceStatus.Online : DeviceStatus.Offline;
            int battery = Convert.ToInt32(hexOfBytesArray[3], 16);
            if(battery > 100) battery = 100;
            Console.Clear();
            Console.WriteLine("============== ARCTIS BATTERY MONITOR =============");
#if DEBUG
            Console.WriteLine($"Device: {arctis.GetFriendlyName()}\nBuffer: {hexOfBytes}\nStatus Hex: {hexOfBytesArray[2]}\nStatus: {status}\nBattery Hex: {hexOfBytesArray[3]}\nBattery: {battery}%");
#else
            Console.WriteLine($"Device: {arctis.GetFriendlyName()}\nStatus: {status}\nBattery: {battery}%");
#endif
            await Task.Delay(1000);
        }
    }
}