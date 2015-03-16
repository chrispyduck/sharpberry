namespace sharpberry.obd.Responses
{
    public enum ResponseStatus
    {
        /// <summary>
        /// The data received does not match what was expected. This indicates a failure.
        /// </summary>
        Invalid,

        /// <summary>
        /// The received data is what was expected
        /// </summary>
        Valid,

        /// <summary>
        /// Still waiting for more data to be received
        /// </summary>
        Incomplete,

        /// <summary>
        /// The status of the command's response is unknown, possible due to a port error, object disposal or intentional disconnection
        /// </summary>
        Indeterminate
    }
}
