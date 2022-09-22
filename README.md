# LimCompiler
## Intro
Lim Compiler is the programming language compiler I am creating.
Please don't look at my horrible code.

![Alt text](https://github.com/GeminoRR/LimCompiler/blob/master/LimCompiler/logo_compiler.ico?raw=true "LimCompiler's logo")

## Lim's particularities
- Totally different reference management *(especially for OOP)*
- Statically Typed
- Compatible with OOP
- Indentation-based statements
- Namespaces (just call spaces)
- Compile to Windows, Linux, MacOS (Using .NET Framework)

## What I hope to add
- Minimal Runtime
- Compile to C and WASM

## Examples
### Hello world
```swift
func main
	puts("Hello world !")
```

### List
```swift
func main
	var city = ["Paris", "Amsterdam", "London", "Kiev"]
	for i in city
		puts("Name : " + i)
```

### Reference
```swift
func main
	var myName = "Bob"
	puts(myName) //Output: Bob
	changeName(myName)
	puts(myName) //Output: Mathis

func changeName(@name) //The "@" is used to pass the value by reference.
	name = "Mathis" //Regardless of the value, changing the "name" variable will affect "myName".
```

###Class
```swift
struct user
	
	var _username:str
	var _age:int

	func create(username:str, age:int) //Constructor
		_username = username
		_age = age

	func str
		return _username + " is " + _age.str()

func main
	current_user = new user("Pierre", 16)
	puts(current_user.str())
```

### Window Graphics
```swift
import graphics

var img:image

func main
	img = loadImage("among_us.png")
	initWindow("My program", 500, 500)

func drawFrame(@screen:image)
	
	//Draw background
	screen.drawRectangle(0, 0, width, height, new color(255, 255, 255))

	//Draw among us image
	screen.drawImage(50, 50, 400, 400, img)
```