namespace Unilocker.Api.DTOs;

// USER

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool Status { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }

}

public class UserCreateUpdateDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public int? RoleId { get; set; }

    // Solo para crear o cambiar contraseña
    public string? Password { get; set; }

    public bool Status { get; set; }
}

// COMPUTER (Uuid como Guid)

public class ComputerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid Uuid { get; set; }
    public bool Status { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public int ClassroomId { get; set; }
    public string ClassroomName { get; set; } = string.Empty;
    public int BlockId { get; set; }
    public string BlockName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
}


public class ComputerCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public Guid Uuid { get; set; }
    public bool Status { get; set; }
    public int ClassroomId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
}


// ROLE

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Status { get; set; }
    public string? Description { get; set; }   // nuevo
}

public class RoleCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }   // nuevo
    public bool Status { get; set; }           // para editar estado
}

// PROBLEM TYPE (nuevo DTO, distinto del modelo)

public class ProblemTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public bool Status { get; set; }
}

public class ProblemTypeCreateUpdateDto
{
    public string Name { get; set; } = null!;
    public bool Status { get; set; }    // nuevo
}