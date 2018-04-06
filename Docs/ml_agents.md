# Using with Unity ml-agents


### Setup
1. Install Unity [ml-agents](https://github.com/Unity-Technologies/ml-agents) (follow their documentation)
2. Add the MujocoBrain (below) to the train_config.yaml file in the python directory
3. In Unity, open hopper or walker from MujocoUnity\Assets\MLA-MujocoUnity\Scenes\
4. Run to see trained brain
5. Set brain to external, build and run from your python path (follow unity ml-agents documentation)


#### Hyperparameters used for walker_w57-3m / 1.5 and hopper_h2-1.5m
```yaml
MujocoBrain:
    beta: 1.0e-4
    epsilon: 0.20
    gamma: 0.99
    lambd: 0.95
    learning_rate: 3.0e-4
    num_epoch: 3
    time_horizon: 128
    summary_freq: 1000
    use_recurrent: false
    normalize: true
    num_layers: 2
    hidden_units: 64
    batch_size: 2048
    buffer_size: 20480
    max_steps: 1e6
```

#### Hyperparameters used for walker_w61-xxx and hopper_h21-xxx (will train faster, less stable)
```yaml
MujocoBrain:
    beta: 5.0e-3
    epsilon: 0.20
    gamma: 0.99
    lambd: 0.95
    learning_rate: 1.0e-3
    num_epoch: 3
    time_horizon: 128
    summary_freq: 1000
    use_recurrent: false
    normalize: true
    num_layers: 2
    hidden_units: 64
    batch_size: 2048
    buffer_size: 10240
    max_steps: 3e5
```

