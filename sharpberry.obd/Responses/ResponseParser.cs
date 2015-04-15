using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sharpberry.obd.Commands;

namespace sharpberry.obd.Responses
{
    public static class ResponseParser
    {
        public static ParsedResponse Parse(string rawResponse, Command sentCommand, ObdFeatures currentFeatures)
        {
            var response = new ParsedResponse();

            switch (rawResponse)
            {
                case "BUFFER FULL":
                case "BUS BUSY":
                case "BUS ERROR":
                case "CAN ERROR":
                case "DATA ERROR":
                case "FB ERROR":
                case "STOPPED":
                case "LV RESET":
                case "UNABLE TO CONNECT":
                    response.Data = rawResponse;
                    response.Error = "Internal ELM error: " + rawResponse;
                    response.IsError = true;
                    return response;
            }

            var standardCommand = sentCommand as ObdCommand;
            if (standardCommand != null)
            {
                // rpm w/o headers:	410C-0E20	
                // rpm w/ headers: 7E804-410C-0E20-2E
                var expectedResponse = (ByteCountExpectedResponse) standardCommand.ExpectedResponse;
                var expectedLength = (currentFeatures.HeadersEnabled ? 11 : 4) + (expectedResponse.NumberOfBytes*2);
                if (rawResponse.Length < expectedLength)
                {
                    response.Status = ResponseStatus.Incomplete;
                    response.Data = rawResponse;
                }
                else if (rawResponse.Length > expectedLength)
                {
                    response.Status = ResponseStatus.Invalid;
                    response.Data = rawResponse;
                }
                else
                {
                    try
                    {
                        var offset = 0;
                        if (currentFeatures.HeadersEnabled)
                            response.Header = rawResponse.Substring(0, (offset += 5)).GetBytesFromHexString();
                        response.Command = rawResponse.Substring(offset, (offset += 4)).GetBytesFromHexString();
                        response.Data = rawResponse.Substring(offset, expectedResponse.NumberOfBytes*2);
                        response.DataBytes = response.Data.GetBytesFromHexString();
                        if (currentFeatures.HeadersEnabled)
                            response.Checksum = rawResponse.Substring(rawResponse.Length - 2, 2).GetBytesFromHexString();
                        response.Status = ResponseStatus.Valid;
                    }
                    catch (Exception e)
                    {
                        response.Status = ResponseStatus.Invalid;
                        response.IsError = true;
                        response.Exception = e;
                        response.Data = e.ToString();
                    }
                }
            }
            else
            {
                // not sure what else to do, so just return the whole mess
                response.Data = rawResponse;
                response.Status = sentCommand.ExpectedResponse.CheckInput(rawResponse);
            }

            return response;
        }
    }
}
