# DistributedFlightControl
Distributed processing system for ADS-B data sourced via EventHub

I got myself a Raspberry Pi w/FlightAware's ADS-B reciever to collect data about the planes flying overhead 
(in flight path for Daytona Beach DAB, Orlando MCO and Miami MIA, plus Embry Riddle university). The FlightAware platform which 
accepts data from many like devices worldwide was an inspiration.

The data is collected from the FlightAware's API and sent to Azure EventHub via sepeate application. I had a processing program
processing this data on my laptop, but wanted to create a distributed system that could scale up to handle a load like FlightAware.

This processing application will run on muliple Raspberry Pis, and coordinate the ingesting and processing of the individual ADS-D 
data packets (1 per plane per second).
