namespace Microsoft.AspNetCore.Authentication.ActiveDirectory
{
    class Constants
    {
        #region Private constants
        private const int ISC_REQ_REPLAY_DETECT = 0x00000004;
        private const int ISC_REQ_SEQUENCE_DETECT = 0x00000008;
        private const int ISC_REQ_CONFIDENTIALITY = 0x00000010;
        private const int ISC_REQ_CONNECTION = 0x00000800;
        #endregion

        #region Public constants
        public const int StandardContextAttributes = ISC_REQ_CONFIDENTIALITY | ISC_REQ_REPLAY_DETECT | ISC_REQ_SEQUENCE_DETECT | ISC_REQ_CONNECTION;
        public const int SecurityNativeDataRepresentation = 0x10;
        public const int MaximumTokenSize = 12288;
        public const int SecurityCredentialsInbound = 1;
        public const int SuccessfulResult = 0;
        public const int IntermediateResult = 0x90312;
        #endregion
    }
}
