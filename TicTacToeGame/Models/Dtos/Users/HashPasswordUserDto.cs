using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TicTacToeGame.Models.Dtos.Users;

public class HashPasswordUserDto
{
    public virtual string Id { get; set; } = default!;
    public virtual string UserName { get; set; } = default!;
    public virtual string Name { get; set; } = default!;
    public virtual string Email { get; set; } = default!;
}