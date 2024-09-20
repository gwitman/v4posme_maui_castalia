using System.Diagnostics;
using Android.Bluetooth;
using Android.Content;
using Java.Util;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace v4posme_maui.Services;

public class BluetoothService(string nameDevice)
{
    private readonly IAdapter _adapter = CrossBluetoothLE.Current.Adapter;

    private static readonly UUID PrinterUuid = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB")!;

    public IDevice? ConnectDevice()
    {
        var device = _adapter.GetSystemConnectedOrPairedDevices().FirstOrDefault(d => d.Name == nameDevice);
        return device;
    }


    private BluetoothDevice? GetDevice(BluetoothAdapter bluetoothAdapter)
    {
        return bluetoothAdapter.BondedDevices!.FirstOrDefault(device => device.Name == nameDevice);
    }

    private BluetoothSocket GetSocket(BluetoothDevice device)
    {
        var socket = device.CreateRfcommSocketToServiceRecord(PrinterUuid)!;
        socket.Connect();
        return socket;
    }

    private static void SendData(byte[] bytes, BluetoothSocket socket)
    {
        var o = socket.OutputStream;
        o!.Write(bytes, 0, bytes.Length);
        o.Flush();
    }

    public void Print(byte[] byteArray)
    {
        try
        {
            if (!CrossBluetoothLE.Current.IsOn)
            {
                return;
            }
            var adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter is null) return;

            Device = GetDevice(adapter);
            //Printer not found
            if (Device is null)
            {
                return;
            }

            var socket = GetSocket(Device);
            SendData(byteArray, socket);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public BluetoothDevice? Device { get; private set; }
}