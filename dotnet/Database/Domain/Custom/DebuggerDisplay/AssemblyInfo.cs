using System.Collections.Generic;
using System.Diagnostics;
using Allors.Database.Domain;
using static DebuggerDisplayConstants;

// General
[assembly: DebuggerDisplay("[Key={Key}, Value={Value}]", Target = typeof(KeyValuePair<,>))]

// Allors
[assembly: DebuggerDisplay("{DebuggerDisplay}" + id, Target = typeof(Grant))]
[assembly: DebuggerDisplay("{Text} [{Locale?.Name}]", Target = typeof(LocalisedText))]
[assembly: DebuggerDisplay("{DisplayName}" + id, Target = typeof(Person))]
[assembly: DebuggerDisplay("{DebuggerDisplay}" + id, Target = typeof(SecurityToken))]
