using System.Diagnostics;
using System.Text;
using Android.Bluetooth;
using Plugin.BLE.Abstractions.Contracts;
using v4posme_maui.Services.HelpersPrinters.Epson_Commands;
using v4posme_maui.Services.HelpersPrinters.Helper;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Printer;
using SkiaSharp;
using Exception = Java.Lang.Exception;
using Fonts = v4posme_maui.Services.HelpersPrinters.Enums.Fonts;
using Positions = v4posme_maui.Services.HelpersPrinters.Enums.Positions;
using PrinterModeState = v4posme_maui.Services.HelpersPrinters.Enums.PrinterModeState;
using QrCodeSize = v4posme_maui.Services.HelpersPrinters.Enums.QrCodeSize;

namespace v4posme_maui.Services.HelpersPrinters
{
    public class Printer : IPrinter
    {
        private byte[] _buffer;
        private readonly string _printerName;
        private readonly IPrintCommand _command;
        private readonly string _codepage;
        private readonly BluetoothService _bluetoothService;

        public Printer(string printerName, string codepage = "IBM860")
        {
            _printerName = string.IsNullOrEmpty(printerName) ? "escpos.prn" : printerName.Trim();
            _command = new EscPos();
            _codepage = codepage;
            _buffer = [];
            _bluetoothService = new BluetoothService(printerName);
        }

        public Printer(string printerName)
        {
            _buffer = [];
            _bluetoothService = new BluetoothService(printerName);
            _printerName = printerName;
            _command = new EscPos();
            _codepage = "IBM860";
        }

        public BluetoothDevice? Device => _bluetoothService.Device;
        public void Print()
        {
            try
            {
                _bluetoothService.Print(_buffer);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        public int ColsNomal => _command.ColsNomal;

        public int ColsCondensed => _command.ColsCondensed;

        public int ColsExpanded => _command.ColsExpanded;

        public void PrintDocument()
        {
            if (_buffer.Length<=0)
                return;
            if (!RawPrinterHelper.SendBytesToPrinter(_printerName, _buffer))
                throw new ArgumentException("Unable to access printer : " + _printerName);
        }

        public void Append(string value)
        {
            AppendString(value, true);
        }

        public void Append(byte[] value)
        {
            if (value.Length<=0)
                return;
            var list = new List<byte>();
            list.AddRange(_buffer);
            list.AddRange(value);
            _buffer = list.ToArray();
        }

        public void AppendWithoutLf(string value)
        {
            AppendString(value, false);
        }

        private void AppendString(string value, bool useLf)
        {
            if (string.IsNullOrEmpty(value))
                return;
            if (useLf)
                value += "\n";
            var list = new List<byte>();
            list.AddRange(_buffer);
            var bytes = Encoding.GetEncoding(_codepage).GetBytes(value);
            list.AddRange(bytes);
            _buffer = list.ToArray();
        }

        public void NewLine()
        {
            Append("\r");
        }

        public void NewLines(int lines)
        {
            for (int i = 1, loopTo = lines - 1; i <= loopTo; i++)
                NewLine();
        }

        public void Clear()
        {
            _buffer = [];
        }

        public void Separator(char speratorChar = '-')
        {
            Append(_command.Separator(speratorChar));
        }

        public void AutoTest()
        {
            Append(_command.AutoTest());
        }

        public void TestPrinter()
        {
            Append("NORMAL - 48 COLUMNS");
            Append("1...5...10...15...20...25...30...35...40...45.48");
            Separator();
            Append("Text Normal");
            BoldMode("Bold Text");
            UnderlineMode("Underlined text");
            Separator();
            ExpandedMode(PrinterModeState.On);
            Append("Expanded - 23 COLUMNS");
            Append("1...5...10...15...20..23");
            ExpandedMode(PrinterModeState.Off);
            Separator();
            CondensedMode(PrinterModeState.On);
            Append("Condensed - 64 COLUMNS");
            Append("1...5...10...15...20...25...30...35...40...45...50...55...60..64");
            CondensedMode(PrinterModeState.Off);
            Separator();
            DoubleWidth2();
            Append("Font Width 2");
            DoubleWidth3();
            Append("Font Width 3");
            NormalWidth();
            Append("Normal width");
            Separator();
            AlignRight();
            Append("Right aligned text");
            AlignCenter();
            Append("Center-aligned text");
            AlignLeft();
            Append("Left aligned text");
            Separator();
            Font("Font A", Fonts.FontA);
            Font("Font B", Fonts.FontB);
            Font("Font C", Fonts.FontC);
            Font("Font D", Fonts.FontD);
            Font("Font E", Fonts.FontE);
            Font("Font Special A", Fonts.SpecialFontA);
            Font("Font Special B", Fonts.SpecialFontB);
            Separator();
            InitializePrint();
            SetLineHeight(24);
            Append("This is first line with line height of 30 dots");
            SetLineHeight(40);
            Append("This is second line with line height of 24 dots");
            Append("This is third line with line height of 40 dots");
            NewLines(3);
            Append("End of Test :)");
            Separator();
        }

        public void BoldMode(string value)
        {
            Append(_command.FontMode.Bold(value));
        }

        public void BoldMode(PrinterModeState state)
        {
            Append(_command.FontMode.Bold(state));
        }

        public void Font(string value, Fonts state)
        {
            Append(_command.FontMode.Font(value, state));
        }

        public void UnderlineMode(string value)
        {
            Append(_command.FontMode.Underline(value));
        }

        public void UnderlineMode(PrinterModeState state)
        {
            Append(_command.FontMode.Underline(state));
        }

        public void ExpandedMode(string value)
        {
            Append(_command.FontMode.Expanded(value));
        }

        public void ExpandedMode(PrinterModeState state)
        {
            Append(_command.FontMode.Expanded(state));
        }

        public void CondensedMode(string value)
        {
            Append(_command.FontMode.Condensed(value));
        }

        public void CondensedMode(PrinterModeState state)
        {
            Append(_command.FontMode.Condensed(state));
        }

        public void NormalWidth()
        {
            Append(_command.FontWidth.Normal());
        }

        public void DoubleWidth2()
        {
            Append(_command.FontWidth.DoubleWidth2());
        }

        public void DoubleWidth3()
        {
            Append(_command.FontWidth.DoubleWidth3());
        }

        public void AlignLeft()
        {
            Append(_command.Alignment.Left());
        }

        public void AlignRight()
        {
            Append(_command.Alignment.Right());
        }

        public void AlignCenter()
        {
            Append(_command.Alignment.Center());
        }

        public void FullPaperCut()
        {
            Append(_command.PaperCut.Full());
        }

        public void PartialPaperCut()
        {
            Append(_command.PaperCut.Partial());
        }

        public void OpenDrawer()
        {
            Append(_command.Drawer.Open());
        }

        public void QrCode(string qrData)
        {
            Append(_command.QrCode.Print(qrData));
        }

        public void QrCode(string qrData, QrCodeSize qrCodeSize)
        {
            Append(_command.QrCode.Print(qrData, qrCodeSize));
        }

        public void Code128(string code, Positions printString = Positions.NotPrint)
        {
            Append(_command.BarCode.Code128(code, printString));
        }
        public void Code128V2(byte[] code, Positions printString = Positions.NotPrint)
        {
            Append(_command.BarCode.Code128V2(code, printString));
        }

        public void Code39(string code, Positions printString = Positions.NotPrint)
        {
            Append(_command.BarCode.Code39(code, printString));
        }

        public void Code39CustomPosMe2px1p(string code, Positions printString = Positions.NotPrint)
        {
            Append(_command.BarCode.Code39CustomPosMe2px1p(code, printString));
        }

        public void Avanza(int puntos)
        {
            Append(_command.Alignment.Avanza(puntos));
        }

        public void Ean13(string code, Positions printString = Positions.NotPrint)
        {
            Append(_command.BarCode.Ean13(code, printString));
        }

        public void InitializePrint()
        {
            RawPrinterHelper.SendBytesToPrinter(_printerName, _command.InitializePrint.Initialize());
        }

        public void Image(SKBitmap image)
        {
            Append(_command.Image.Print(image));
        }

        public void NormalLineHeight()
        {
            Append(_command.LineHeight.Normal());
        }

        public void SetLineHeight(byte height)
        {
            Append(_command.LineHeight.SetLineHeight(height));
        }

        public void Image(byte[] logoByte)
        {
            Append(logoByte);
        }
    }
}