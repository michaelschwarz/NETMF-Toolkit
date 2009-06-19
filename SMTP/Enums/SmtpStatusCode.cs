using System;
using System.Collections.Generic;
using System.Text;

namespace MSchwarz.Net.Mail
{
    public enum SmtpStatusCode
    {
        GeneralFailure = -1,

        /// <summary>
        /// System status, or system help reply
        /// </summary>
        SystemStatus = 211,

        /// <summary>
        /// Help message
        /// </summary>
        HelpMessage = 214,

        /// <summary>
        /// Service ready
        /// </summary>
        ServiceReady = 220,

        /// <summary>
        /// Service closing transmission channel
        /// </summary>
        ServiceClosingTransmissionChannel = 221,

        /// <summary>
        /// Requested mail action okay, completed
        /// </summary>
        Ok = 250,

        /// <summary>
        /// User not local; will forward to &lt;forward-path&gt;
        /// </summary>
        UserNotLocalWillForward = 251,

        /// <summary>
        /// 
        /// </summary>
        CannotVerifyUserWillAttemptDelivery = 252,

        /// <summary>
        /// Start mail input; end with &lt;CRLF&gt;.&lt;CRLF&gt;
        /// </summary>
        StartMailInput = 354,

        /// <summary>
        /// Service not available, closing transmission channel
        /// </summary>
        ServiceNotAvailable = 421,

        /// <summary>
        /// Requested mail action not taken: mailbox unavailable
        /// </summary>
        MailboxBusy = 450,

        /// <summary>
        /// Requested action aborted: local error in processing
        /// </summary>
        LocalErrorInProcessing = 451,

        /// <summary>
        /// Requested action not taken: insufficient system storage
        /// </summary>
        InsufficientStorage = 452,

        /// <summary>
        /// 
        /// </summary>
        ClientNotPermitted = 454,

        /// <summary>
        /// Syntax error, command unrecognized
        /// </summary>
        CommandUnrecognized = 500,

        /// <summary>
        /// Syntax error in parameters or arguments
        /// </summary>
        SyntaxError = 501,

        /// <summary>
        /// Command not implemented
        /// </summary>
        CommandNotImplemented = 502,

        /// <summary>
        /// Bad sequence of commands
        /// </summary>
        BadCommandSequence = 503,

        /// <summary>
        /// Command parameter not implemented
        /// </summary>
        CommandParameterNotImplemented = 504,

        /// <summary>
        /// Access denied
        /// </summary>
        MustIssueStartTlsFirst = 530,

        /// <summary>
        /// SMTP Authentication unsuccessful/Bad username or password
        /// </summary>
        AuthenticationUnsuccessful = 535,

        /// <summary>
        /// Requested action not taken: mailbox unavailable
        /// </summary>
        MailboxUnavailable = 550,

        /// <summary>
        /// User not local; please try &lt;forward-path&gt;
        /// </summary>
        UserNotLocalTryAlternatePath = 551,

        /// <summary>
        /// Requested mail action aborted: exceeded storage allocation
        /// </summary>
        ExceededStorageAllocation = 552,

        /// <summary>
        /// Requested action not taken: mailbox name not allowed
        /// </summary>
        MailboxNameNotAllowed = 553,

        /// <summary>
        /// Transaction failed
        /// </summary>
        TransactionFailed = 554,
    }
}