using System;
using System.Collections.Generic;
using System.Text;

namespace PoppuloAPI
{
    /// <summary>
    /// enum to handle Http Verbs.
    /// </summary>
    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    /// <summary>
    /// enum to handle autentication type.
    /// </summary>
    public enum AuthenticationType
    {
        Basic,
        NTLM
    }
}

/// <summary>
/// enum to handle subscriber status.
/// </summary>
public enum SubscriberStatus
{
    active,
    inactive,
    opt_out,
    all
}

/// <summary>
/// enum to handle subscriber genders.
/// </summary>
public enum SubscriberGender
{
    Male,
    Female,
}
