# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  # Загальні налаштування
  config.vm.synced_folder ".", "/vagrant", disabled: false

  # ===========================
  # BaGet Server (NuGet Repository)
  # ===========================
  config.vm.define "baget", primary: true do |baget|
    baget.vm.box = "ubuntu/focal64"
    baget.vm.hostname = "baget-server"
    baget.vm.network "private_network", ip: "192.168.56.2"
    baget.vm.network "forwarded_port", guest: 5000, host: 5000
    
    baget.vm.provider "virtualbox" do |vb|
      vb.name = "FintechStats-BaGet"
      vb.memory = "1024"
      vb.cpus = 1
    end
    
    baget.vm.provision "file", source: "./nupkg", destination: "/home/vagrant/nupkg"
    
    baget.vm.provision "shell", inline: <<-SHELL
      echo "=== Встановлення Docker ==="
      sudo apt-get update
      sudo apt-get install -y docker.io curl
      sudo systemctl start docker
      sudo systemctl enable docker
      sudo usermod -aG docker vagrant
      
      echo "=== Запуск BaGet сервера ==="
      sudo docker run -d \
        --restart unless-stopped \
        -p 5000:80 \
        -e ApiKey=NUGET-SERVER-API-KEY \
        -e Storage__Type=FileSystem \
        -e Storage__Path=/var/baget/packages \
        -e Database__Type=Sqlite \
        -e Database__ConnectionString="Data Source=/var/baget/baget.db" \
        -e Search__Type=Database \
        -v /home/vagrant/baget-data:/var/baget \
        --name baget \
        loicsharma/baget:latest
      
      echo "=== Очікування запуску BaGet ==="
      sleep 15
      
      until curl -s http://localhost:5000/v3/index.json > /dev/null; do
        echo "Очікування BaGet..."
        sleep 5
      done
      
      echo "=== Завантаження NuGet пакету на BaGet ==="
      cd /home/vagrant/nupkg
      for pkg in FintechStatsPlatform.*.nupkg; do
        if [[ "$pkg" != *"symbols"* ]]; then
          curl -X PUT \
            -H "X-NuGet-ApiKey: NUGET-SERVER-API-KEY" \
            -F "package=@$pkg" \
            http://localhost:5000/api/v2/package
        fi
      done
      
      echo "✅ BaGet сервер готовий: http://192.168.56.2:5000"
      echo "✅ З хост-машини: http://localhost:5000"
    SHELL
  end

  # ===========================
  # Ubuntu 20.04 VM
  # ===========================
  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "ubuntu/focal64"
    ubuntu.vm.hostname = "fintech-ubuntu"
    ubuntu.vm.network "private_network", ip: "192.168.56.10"
    ubuntu.vm.network "forwarded_port", guest: 5001, host: 5001
    
    ubuntu.vm.provider "virtualbox" do |vb|
      vb.name = "FintechStats-Ubuntu"
      vb.memory = "2048"
      vb.cpus = 2
      vb.gui = true
    end
    
    ubuntu.vm.provision "shell", inline: <<-SHELL
      echo "=== Встановлення .NET SDK 8.0 ==="
      wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      rm packages-microsoft-prod.deb
      sudo apt-get update
      sudo apt-get install -y apt-transport-https dotnet-sdk-8.0
      
      echo "=== Встановлення PostgreSQL ==="
      sudo apt-get install -y postgresql postgresql-contrib
      sudo systemctl start postgresql
      sudo systemctl enable postgresql
      
      sudo -u postgres psql -c "CREATE DATABASE fintechstats;" || true
      sudo -u postgres psql -c "CREATE USER fintechuser WITH PASSWORD 'fintechpass';" || true
      sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE fintechstats TO fintechuser;" || true
      
      echo "=== Очікування BaGet ==="
      until curl -s http://192.168.56.2:5000/v3/index.json > /dev/null; do
        echo "Очікування BaGet..."
        sleep 5
      done
      
      echo "=== Налаштування NuGet ==="
      sudo -u vagrant dotnet nuget add source http://192.168.56.2:5000/v3/index.json \
        --name BaGet || true
      
      echo "✅ Ubuntu VM готова!"
      echo "Підключення: vagrant ssh ubuntu"
      echo "Запуск: cd /vagrant/Zenflow && dotnet run --urls http://0.0.0.0:5001"
    SHELL
  end

  # ===========================
  # CentOS Stream 9 VM
  # ===========================
  config.vm.define "centos" do |centos|
    centos.vm.box = "generic/centos9s"
    centos.vm.hostname = "fintech-centos"
    centos.vm.network "private_network", ip: "192.168.56.11"
    centos.vm.network "forwarded_port", guest: 5002, host: 5002
    
    centos.vm.provider "virtualbox" do |vb|
      vb.name = "FintechStats-CentOS"
      vb.memory = "2048"
      vb.cpus = 2
    end
    
    centos.vm.provision "shell", inline: <<-SHELL
      echo "=== Встановлення .NET SDK 8.0 ==="
      sudo dnf install -y dotnet-sdk-8.0
      
      echo "=== Встановлення PostgreSQL ==="
      sudo dnf install -y postgresql-server postgresql-contrib
      sudo postgresql-setup --initdb || true
      sudo systemctl start postgresql
      sudo systemctl enable postgresql
      
      echo "=== Очікування BaGet ==="
      until curl -s http://192.168.56.2:5000/v3/index.json > /dev/null 2>&1; do
        echo "Очікування BaGet..."
        sleep 5
      done
      
      echo "=== Налаштування NuGet ==="
      sudo -u vagrant dotnet nuget add source http://192.168.56.2:5000/v3/index.json \
        --name BaGet || true
      
      echo "✅ CentOS VM готова!"
      echo "Підключення: vagrant ssh centos"
      echo "Запуск: cd /vagrant/Zenflow && dotnet run --urls http://0.0.0.0:5002"
    SHELL
  end
end