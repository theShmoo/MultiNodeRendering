% VU Echtzeit-Visualisierung
% Bernhard Rainer; David Pfahler
% __Team\_TUtor__

# Remote rendering

The main idea of remote rendering is to distribute the rendering onto multiple nodes. A client splits its screen into multiple tiles, and each buffer for each tile is calculated by a different node. These calculated buffers are stacked back together by the client and it calculates the final illumination.

## The Nodes

Each node is responsible for one tile of the entire screen and generates only a buffer. The input data for all nodes is the same but every node has a different output which is sent to the client. For example a depth buffer, normal buffer or object buffer. 

## The Data

The data is synchronized on all nodes. And is too complex to be rendered on only one client machine.

## The Client

The client is responsible for dividing the rendering stages onto its nodes and to compose the final image by receiving the calculated buffers from the nodes, stacking them together and calculate the illumination.

## Frameworks

 * Unity
 * [Cluster Rendering](https://bitbucket.org/Unity-Design/clusterrendering)