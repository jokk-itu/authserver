using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.PostUserLogin;
public class PostUserLoginRequest
{
  public string Username { get; set; }

  public string Password { get; set; }
}
