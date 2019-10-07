# Tilde Server and CLI 

### Installation 
 
Install tilde server and CLI as a global dotnet tool from nuget
https://www.nuget.org/packages/tilde 

To install these tools use the **dotnet tool install** command.

```
dotnet tool install --global tilde
```

To update to the latest version of one of these tools use the **dotnet tool update** command.
```
dotnet tool install --global tilde
```


### Run tilde server inside a docker container

Pull the tilde server docker image from docker hub. 
https://hub.docker.com/r/tildelove/tilde

The dockerfile is maintained by the Tilde Docker repository
https://github.com/tilde-love/tilde-docker

```
docker pull tildelove/tilde
```


In this example the relative path `./tilde-projects` is being mapped to `/projects` inside the container. 
Additionally port 5678 is exposed   

```
docker run -v ./tilde-projects:/projects -p 5678:5678 tildelove/tilde
```


# Commands

### Serve 

To start a tilde server on the default uri `http://localhost:5678/`

```
tilde serve ./tilde-projects 
```

## Commands that run against a tilde server instance.

### Create a tilde project 
```
tilde new project NewProject 
```
### List projects 
```
tilde ls projects  
```

### Help 

```
tilde --help 
```

```
tilde:
  Tilde Love - Containerisation for artists.

Usage:
  tilde [command]

Commands:
  add                 Add a resource.
  del, delete         Delete a resource.
  get                 Get a resource.
  ls, list            Lists a resource.
  logs                Get resource logs from a remote location.
  n, new, create      Create a new resource.
  pack <paths>        Create and pack a template index from a directory in the file system.
  pull                Pull a resource from a remote location.
  push                Push a resource to a remote location.
  rm, remove          Remove a resource.
  serve <projects>    Start a tilde server instance. Equivalent of running "tilde start server"
  set                 Set a resource.
  start               Start a resource process or service.
  stop                Stop a resource process or service.
  unpack <paths>      Unpack a template index to a directory in the file system.
  watch               Watch a local a resource and push any changes to a remote location. process or service.
```

... more
