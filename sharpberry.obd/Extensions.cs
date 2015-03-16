using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace sharpberry.obd
{
    public static class Extensions
    {
        public static byte[] GetBytesFromHexString(this string input)
        {
            try
            {
                if (input == null) // no data received yet
                    return new byte[0];
                var charCount = input.Length;
                if (charCount == 0) // no data received yet
                    return new byte[0];
                var bytes = new byte[charCount/2 + charCount%2];
                for (var i = charCount - 1; i > -1; i -= 2)
                    bytes[i/2] = Convert.ToByte(i == 0 ? input.Substring(0, 1) : input.Substring(i - 1, 2), 16);
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        public static string ToHexString(this byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }

        public static Task AsTask(this WaitHandle handle)
        {
            return AsTask(handle, Timeout.InfiniteTimeSpan);
        }

        public static Task AsTask(this WaitHandle handle, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<object>();
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (state, timedOut) =>
            {
                var localTcs = (TaskCompletionSource<object>)state;
                localTcs.SetResult(null);
            }, tcs, timeout, executeOnlyOnce: true);
            tcs.Task.ContinueWith((_, state) => ((RegisteredWaitHandle)state).Unregister(null), registration, TaskScheduler.Default);
            return tcs.Task;
        }
    }
}
