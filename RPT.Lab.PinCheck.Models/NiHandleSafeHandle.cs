using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RPT.Lab.PinCheck.Models.NI845X;

namespace RPT.Lab.PinCheck.Models
{
    public class NiHandleSafeHandle : SafeHandle
    {
        public NiHandleSafeHandle() : base(IntPtr.Zero, true) { }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            // Call ni845xClose or appropriate cleanup function
            int result = ni845xClose(new NiHandleSafeHandle { handle = handle });
            return result == (int)Ni845xError.SUCCESS;
        }
    }

}
