using System;
using System.Reflection;
[assembly: AssemblyVersion("0.1.26.1")]
[assembly: ChangesetAttribute("26", "309926075e08", "default")]

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