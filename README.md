This is a code analysis tool that performs the following operations:
  1. Parse C# source code and identify tokens.
  2. From the tokens obtained, identify all the types in the file, like class, function, delegates, namespace, aliases, etc.
  3. From the types, analyse the dependency between a pair of files in the file set.
  4. Identify the strong components from the dependency analysis using Tarjan's algorithm.
  
  It is designed as a client server model using Windows Communication Foundation, and the GUI is created using Windows Presentation Foundation. 
