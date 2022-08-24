# LimCompiler
## Intro
Lim Compiler is the programming language compiler I am creating.
Please don't look at my horrible code.

![Alt text](https://github.com/GeminoRR/LimCompiler/blob/master/LimCompiler/logo_compiler.ico?raw=true "LimCompiler's logo")

## The difference between other languages and Lim
- Different management of references
- Statically Typed
- Minimal Runtime
- Object-oriented programming
- Handle statement with indentation
- Compiles to C, Executable Windows and Linux (later wasm)

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
	var myList = ["un", "deu", "trois", "quatre"]
	puts(myList) //Output : ["un", "deu", "trois", "quatre"]
	changeAt(myList, 1, "deux")
	puts(myList) //Output : ["un", "deux", "trois", "quatre"]

func changeAt(@list:str[], index:int, newValue:str)
	//The "@" allows values to be passed by reference. The send list will therefore be modified, otherwise it would have been copied entirely.
	list[index] = newValue
```

### Window Graphics
```swift
import graphics

var img:image

func main
	img = loadImage("amogous.png")
	initWindow("My program", 500, 500)

func drawFrame
	
	//Draw background
	drawRectangle(0, 0, width, height, "#FFFFFF")

	//Draw amogous
	drawImage(50, 50, 400, 400, img)
```
