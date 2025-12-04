namespace Unilocker.Api.DTOs;

public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? Code { get; set; }
    public bool Status { get; set; }
}

public class BranchCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? Code { get; set; }

    public bool Status { get; set; }
}

public class BlockDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Status { get; set; }
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
}

public class BlockCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public int BranchId { get; set; }

    public bool status { get; set; }
}

public class ClassroomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Status { get; set; }
    public int BlockId { get; set; }
    public string BlockName { get; set; } = null!;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = null!;
}

public class ClassroomCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public int BlockId { get; set; }
}
