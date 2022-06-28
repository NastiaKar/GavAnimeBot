using Microsoft.AspNetCore.Http;

namespace basedApi.Models;

public class AnimeModel
{
  public Data Data { get; set; }
}

public class AnimeModelArray
{
  public IEnumerable<Data> Data { get; set; }
}

public class Data
{
  public string Id { get; set; }
  public Attributes Attributes { get; set; }
}

public class Attributes
{
  public string CreatedAt { get; set; }

  public string Name { get; set; }
  public string Synopsis { get; set; }
  public string AgeRating { get; set; }
  public string AgeRatingGuide { get; set; }
  public Titles Titles { get; set; }
  public PosterImage PosterImage { get; set; }
}

public class Titles
{
  public string En_Jp { get; set; }
  public string Ja_Jp { get; set; }
}

public class PosterImage
{
  public string Large { get; set; }
}