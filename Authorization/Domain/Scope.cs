﻿namespace Domain;

#nullable disable
public class Scope
{
  public int Id { get; set; }

  public string Name { get; set; }

  public ICollection<Resource> Resources { get; set; }

  public ICollection<Client> Clients { get; set; }

  public ICollection<Grant> Grants { get; set; }
}