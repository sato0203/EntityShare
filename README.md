# EntityShare
Compile C#EntityClass to TypeScript to develop WebApp with ASP.NET.Core and TypeScript frontend

## How To Use

```
dotnet run [src] [dest]
```
- [src] is a folder path has C# source files;
- [dest] is a filepath for result

## Example
An example of compiling three C # source files into a Typescript source file
- C# Article
```
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Model
{
    public class Article
    {
        public virtual long Id { get; set; }
        [Required]
        public virtual User User { get; set; }
        [Required]
        public virtual Shop Shop { get; set; }
        [Required]
        public virtual string Text { get; set; }
        [Required]
        public virtual string ImageURL { get; set; }
    }
}
```
- C# User
```
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Model
{
    public class User
    {
        public virtual long Id { get; set; }
        [Required]
        public virtual string Name { get; set; }
    }
}
```
- C# Shop
```
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace API.Model
{
    public class Shop
    {
        public virtual long Id { get; set; }
        [Required]
        public virtual string Name { get; set; }
        [Required]
        public virtual double Latitude { get; set; }
        [Required]
        public virtual double Longitude { get; set; }
    }
}

```
- Typescirpt Source
```
export interface Article {
	Id:number
	User:User
	Shop:Shop
	Text:string
	ImageURL:string
}

export interface Shop {
	Id:number
	Name:string
	Latitude:number
	Longitude:number
}

export interface User {
	Id:number
	Name:string
}

```

## License
MIT License
