# spike-fsharp-covid19
Spike to try to learn F#, Fable, F# Fake, Farmer


#Docker cli 

- Build a docker container by specifing a repo (-t) name and docker file (-f) 
```Yaml
docker build -t covid-19-image  -f .\Dockerfile .
``` 

- run a docker container -p port --rm remove container after stop -it interactive terminal
```Yaml
docker run -p 5000:5000 --rm -it covid-19-image /bin/bash 
``` 

- run docker and mount directory from host sytem into docker container
```Yaml
docker run --mount type=bind,source=/c/workspace/thechris/fsharp-covid19/data,target=/data/ -p 5000:5000 --rm -it covid-19-image
```

