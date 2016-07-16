% VU Echtzeit-Visualisierung
% Bernhard Rainer; David Pfahler
% __Team\_TUtor__

# Remote Rendering

The main idea of remote rendering is to distribute the rendering onto multiple nodes. A client splits its screen into multiple tiles, and each buffer for each tile is calculated by a different node. These calculated buffers are stacked back together by the client and it calculates the final illumination.

## The Nodes

Each node is responsible for one tile of the entire screen and generates only a buffer. The input data for all nodes is the same but every node has a different output which is sent to the client. For example a depth buffer, normal buffer or object buffer. 

## The Data

The data is synchronized on all nodes. And is too complex to be rendered on only one client machine.

## The Client

The client is responsible for dividing the rendering stages onto its nodes and to compose the final image by receiving the calculated buffers from the nodes, stacking them together and calculate the illumination.

## Frameworks

 * Unity

## Creating a Volume Texture
To create a Volume Texture a sequence of images is needed. The dataset must be stored in in one directory. Start the VolumeGenerator scene in Unity and set the (relative) directory of the images to as the dir parameter. This will load all textures from the directory and construct a 3D Texture, containing the volume. The texture is stored in the /Generated folder of the Unity project. This 3D-texture can then be used for Raycasting by setting the VolumeTexture property of the TileRaycaster with this texture. The application provides to sample datasets located in the Resources folder. 