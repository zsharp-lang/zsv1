import { UUID } from "std:uuid";

let WorkspaceID = newType(UUID);

class Workspace 
    : UniqueIDMixin[WorkspaceID]
    , AuditAllMixin
    of ORMModel(table: "workspaces")
{
    @bind(.Static)
    let uniqueWorkspaceNamePerAccount = UniqueConstraint(
        name: "Unique Workspace Name Per Account",
        columns: [accountId, name],
        where: not Workspace.isDeleted,
    );

    var name: String(min=1, max=255, regex="^[a-z0-9-]+$");
    var title: String(min=1, max=255);

    @foreignKey(
        onDelete: .Cascade,
    )
    @index
    @protectedSetter(Workspace) // private settable field.
    var accountId: AccountID;

    @relationship(
        strategy: .Select,
    )
    var account: Account;
}
