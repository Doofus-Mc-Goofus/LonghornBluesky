using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    internal static class ShellHelper
    {
        [ComImport]
        [Guid("00000122-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IDropTarget
        {
            int DragEnter(
                [In] System.Runtime.InteropServices.ComTypes.IDataObject pDataObj,
                [In] int grfKeyState,
                [In] Point pt,
                [In, Out] ref int pdwEffect);

            int DragOver(
                [In] int grfKeyState,
                [In] Point pt,
                [In, Out] ref int pdwEffect);

            int DragLeave();

            int Drop(
                [In] System.Runtime.InteropServices.ComTypes.IDataObject pDataObj,
                [In] int grfKeyState,
                [In] Point pt,
                [In, Out] ref int pdwEffect);
        }

        internal static void PrintPhotosWizard(string p_FileName)
        {
            IDataObject v_DataObject = new DataObject(DataFormats.FileDrop, new string[] { p_FileName });
            MemoryStream v_MemoryStream = new MemoryStream(4);
            byte[] v_Buffer = new byte[] { (byte)5, 0, 0, 0 };
            v_MemoryStream.Write(v_Buffer, 0, v_Buffer.Length);
            v_DataObject.SetData("Preferred DropEffect", v_MemoryStream);
            Guid CLSID_PrintPhotosDropTarget = new Guid("60fd46de-f830-4894-a628-6fa81bc0190d");
            Type v_DropTargetType = Type.GetTypeFromCLSID(CLSID_PrintPhotosDropTarget, true);
            IDropTarget v_DropTarget = (IDropTarget)Activator.CreateInstance(v_DropTargetType);
            v_DropTarget.Drop((System.Runtime.InteropServices.ComTypes.IDataObject)v_DataObject, 0, new Point(), 0);
        }
    }
}
