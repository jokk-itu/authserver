#! bin/sh

echo 'Setting up firewall...'

ufw allow 22
ufw allow 80
ufw allow 443
ufw enable -y

echo 'Firewall enabled for ports 22, 80 and 443'

echo 'Setting up docker...'

sudo apt-get -y update
sudo apt-get -y install ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg
echo "deb [arch="$(dpkg --print-architecture)" signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu "$(. /etc/os-release && echo "$VERSION_CODENAME")" stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo 'Docker setup completed'
echo 'Docker installed with version $(docker -v)'
echo 'Docker Compose installed with version $(docker compose version)'

echo 'Installing dotnet-sdk'
sudo apt-get install -y dotnet-sdk-6.0
dotnet tool install --global dotnet-ef --version 6.0.16
echo 'Installed dotnet-sdk with version $(dotnet -v)'