start "StateCenter" Iveely.CloudComputting.StateCenter.exe
start "Worker:8001" Iveely.CloudComputting.Worker.exe 8001
start "Worker:8002" Iveely.CloudComputting.Worker.exe 8002
start "Merger" Iveely.CloudComputting.Merger.exe
start "Cacher" Iveely.CloudComputting.Cache.exe