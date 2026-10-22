using System.Runtime.CompilerServices;

// The EditMode tests in Assets/Editor/Tests compile into Assembly-CSharp-Editor. This lets them use the
// game's internal members, so renaming one breaks the tests at compile time instead of at run time.
[assembly: InternalsVisibleTo("Assembly-CSharp-Editor")]
