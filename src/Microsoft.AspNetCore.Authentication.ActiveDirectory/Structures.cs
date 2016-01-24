namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityHandle
    {
        public IntPtr LowPart;
        public IntPtr HighPart;

        public SecurityHandle(int dummy)
        {
            LowPart = HighPart = IntPtr.Zero;
        }

        /// <summary>
        /// Resets all internal pointers to default value
        /// </summary>
        public void Reset()
        {
            LowPart = HighPart = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityInteger
    {
        public uint LowPart;
        public int HighPart;
        public SecurityInteger(int dummy)
        {
            LowPart = 0;
            HighPart = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityBuffer : IDisposable
    {
        public int cbBuffer;
        public int cbBufferType;
        public IntPtr pvBuffer;

        public SecurityBuffer(int bufferSize)
        {
            cbBuffer = bufferSize;
            cbBufferType = (int)SecurityBufferType.SECBUFFER_TOKEN;
            pvBuffer = Marshal.AllocHGlobal(bufferSize);
        }

        public SecurityBuffer(byte[] secBufferBytes)
        {
            cbBuffer = secBufferBytes.Length;
            cbBufferType = (int)SecurityBufferType.SECBUFFER_TOKEN;
            pvBuffer = Marshal.AllocHGlobal(cbBuffer);
            Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
        }

        public SecurityBuffer(byte[] secBufferBytes, SecurityBufferType bufferType)
        {
            cbBuffer = secBufferBytes.Length;
            cbBufferType = (int)bufferType;
            pvBuffer = Marshal.AllocHGlobal(cbBuffer);
            Marshal.Copy(secBufferBytes, 0, pvBuffer, cbBuffer);
        }

        public void Dispose()
        {
            if (pvBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pvBuffer);
                pvBuffer = IntPtr.Zero;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SecurityBufferDesciption : IDisposable
    {

        public int ulVersion;
        public int cBuffers;
        public IntPtr pBuffers; //Point to SecBuffer

        public SecurityBufferDesciption(int bufferSize)
        {
            ulVersion = (int)SecurityBufferType.SECBUFFER_VERSION;
            cBuffers = 1;
            SecurityBuffer ThisSecBuffer = new SecurityBuffer(bufferSize);
            pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(ThisSecBuffer));
            Marshal.StructureToPtr(ThisSecBuffer, pBuffers, false);
        }

        public SecurityBufferDesciption(byte[] secBufferBytes)
        {
            ulVersion = (int)SecurityBufferType.SECBUFFER_VERSION;
            cBuffers = 1;
            SecurityBuffer ThisSecBuffer = new SecurityBuffer(secBufferBytes);
            pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(ThisSecBuffer));
            Marshal.StructureToPtr(ThisSecBuffer, pBuffers, false);
        }

        public SecurityBufferDesciption(BufferWrapper[] secBufferBytesArray)
        {
            if (secBufferBytesArray == null || secBufferBytesArray.Length == 0)
            {
                throw new ArgumentException("secBufferBytesArray cannot be null or 0 length");
            }

            ulVersion = (int)SecurityBufferType.SECBUFFER_VERSION;
            cBuffers = secBufferBytesArray.Length;

            //Allocate memory for SecBuffer Array....
            pBuffers = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Buffer)) * cBuffers);

            for (int Index = 0; Index < secBufferBytesArray.Length; Index++)
            {
                //Super hack: Now allocate memory for the individual SecBuffers
                //and just copy the bit values to the SecBuffer array!!!
                SecurityBuffer ThisSecBuffer = new SecurityBuffer(secBufferBytesArray[Index].Buffer, secBufferBytesArray[Index].BufferType);

                //We will write out bits in the following order:
                //int cbBuffer;
                //int BufferType;
                //pvBuffer;
                //Note that we won't be releasing the memory allocated by ThisSecBuffer until we
                //are disposed...
                int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                Marshal.WriteInt32(pBuffers, CurrentOffset, ThisSecBuffer.cbBuffer);
                Marshal.WriteInt32(pBuffers, CurrentOffset + Marshal.SizeOf(ThisSecBuffer.cbBuffer), ThisSecBuffer.cbBufferType);
                Marshal.WriteIntPtr(pBuffers, CurrentOffset + Marshal.SizeOf(ThisSecBuffer.cbBuffer) + Marshal.SizeOf(ThisSecBuffer.cbBufferType), ThisSecBuffer.pvBuffer);
            }
        }

        public void Dispose()
        {
            if (pBuffers != IntPtr.Zero)
            {
                if (cBuffers == 1)
                {
                    SecurityBuffer ThisSecBuffer = (SecurityBuffer)Marshal.PtrToStructure(pBuffers, typeof(SecurityBuffer));
                    ThisSecBuffer.Dispose();
                }
                else
                {
                    for (int Index = 0; Index < cBuffers; Index++)
                    {
                        //The bits were written out the following order:
                        //int cbBuffer;
                        //int BufferType;
                        //pvBuffer;
                        //What we need to do here is to grab a hold of the pvBuffer allocate by the individual
                        //SecBuffer and release it...
                        int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                        IntPtr SecBufferpvBuffer = Marshal.ReadIntPtr(pBuffers, CurrentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int)));
                        Marshal.FreeHGlobal(SecBufferpvBuffer);
                    }
                }

                Marshal.FreeHGlobal(pBuffers);
                pBuffers = IntPtr.Zero;
            }
        }

        public byte[] GetBytes()
        {
            byte[] Buffer = null;

            if (pBuffers == IntPtr.Zero)
            {
                throw new InvalidOperationException("Object has already been disposed!!!");
            }

            if (cBuffers == 1)
            {
                SecurityBuffer ThisSecBuffer = (SecurityBuffer)Marshal.PtrToStructure(pBuffers, typeof(SecurityBuffer));

                if (ThisSecBuffer.cbBuffer > 0)
                {
                    Buffer = new byte[ThisSecBuffer.cbBuffer];
                    Marshal.Copy(ThisSecBuffer.pvBuffer, Buffer, 0, ThisSecBuffer.cbBuffer);
                }
            }
            else
            {
                int BytesToAllocate = 0;

                for (int Index = 0; Index < cBuffers; Index++)
                {
                    //The bits were written out the following order:
                    //int cbBuffer;
                    //int BufferType;
                    //pvBuffer;
                    //What we need to do here calculate the total number of bytes we need to copy...
                    int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                    BytesToAllocate += Marshal.ReadInt32(pBuffers, CurrentOffset);
                }

                Buffer = new byte[BytesToAllocate];

                for (int Index = 0, BufferIndex = 0; Index < cBuffers; Index++)
                {
                    //The bits were written out the following order:
                    //int cbBuffer;
                    //int BufferType;
                    //pvBuffer;
                    //Now iterate over the individual buffers and put them together into a
                    //byte array...
                    int CurrentOffset = Index * Marshal.SizeOf(typeof(Buffer));
                    int BytesToCopy = Marshal.ReadInt32(pBuffers, CurrentOffset);
                    IntPtr SecBufferpvBuffer = Marshal.ReadIntPtr(pBuffers, CurrentOffset + Marshal.SizeOf(typeof(int)) + Marshal.SizeOf(typeof(int)));
                    Marshal.Copy(SecBufferpvBuffer, Buffer, BufferIndex, BytesToCopy);
                    BufferIndex += BytesToCopy;
                }
            }

            return (Buffer);
        }
    }


    struct BufferWrapper
    {
        public byte[] Buffer;
        public SecurityBufferType BufferType;

        public BufferWrapper(byte[] buffer, SecurityBufferType bufferType)
        {
            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("buffer cannot be null or 0 length");
            }

            Buffer = buffer;
            BufferType = bufferType;
        }
    };

}
