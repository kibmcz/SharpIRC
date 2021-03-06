using System;
using System.Reflection;
[assembly: AssemblyVersion("0.1.28.1")]
[assembly: ChangesetAttribute("28", "d4d83f5d9eff", "default")]

[AttributeUsage(AttributeTargets.Assembly)]
public class ChangesetAttribute : Attribute {
    public string RevisionNumber { get; private set; }
    public string RevisionID { get; private set; }
    public string Branch { get; private set; }

    public ChangesetAttribute(string revisionNum, string revisionId, string branch) {
        RevisionNumber = revisionNum;
        RevisionID = revisionId;
        Branch = branch;
    }
}