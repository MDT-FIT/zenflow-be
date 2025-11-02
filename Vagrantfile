# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  # Загальні налаштування:
  # Синхронізуєм поточну папку з папкою /vagrant всередині VM
  config.vm.synced_folder ".", "/vagrant", disabled: false

  # ===========================
  # Ubuntu 20.04 VM
  # ===========================
  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "ubuntu/focal64"
    ubuntu.vm.hostname = "fintech-ubuntu"
    ubuntu.vm.network "private_network", ip: "192.168.56.10"
    # Додаток з Ubuntu - http://localhost:15001/swagger
    ubuntu.vm.network "forwarded_port", guest: 5001, host: 15001

    ubuntu.vm.provider "virtualbox" do |vb|
      vb.name = "FintechStats-Ubuntu"
      vb.memory = "2048"
      vb.cpus = 2
      vb.gui = true
    end

    ubuntu.vm.provision "shell", inline: <<-SHELL
      
      # Шлях до папки з .csproj файлом всередині VM
      PROJECT_DIR="/vagrant/Zenflow" 
      
      echo "=== Встановлення .NET SDK 8.0 (Ubuntu) ==="
      wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      rm packages-microsoft-prod.deb
      sudo apt-get update
      sudo apt-get install -y apt-transport-https dotnet-sdk-8.0

      echo "=== Встановлення PostgreSQL (Ubuntu) ==="
      sudo apt-get install -y postgresql postgresql-contrib
      sudo systemctl start postgresql
      sudo systemctl enable postgresql

      echo "=== Налаштування БД ==="
      sudo -u postgres psql -c "CREATE DATABASE fintechstats;" || true
      sudo -u postgres psql -c "CREATE USER fintechuser WITH PASSWORD 'fintechpass';" || true
      sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE fintechstats TO fintechuser;" || true

      echo "=== Збірка та запуск додатку (Ubuntu) ==="
      cd $PROJECT_DIR
      
      echo "=== Виконуємо dotnet build ==="
      dotnet build
      
      echo "Запускаємо додаток у фоновому режимі на порту 5001"
      nohup dotnet run --urls "http://0.0.0.0:5001" > /home/vagrant/app.log 2>&1 &
      
      echo "✅ Ubuntu VM готова! Додаток запущено."
      echo "✅ Доступно на вашому комп'ютері: http://localhost:15001/swagger"
    SHELL
  end

  # ===========================
  # CentOS Stream 9 VM
  # ===========================
  config.vm.define "centos" do |centos|
    centos.vm.box = "generic/centos9s"
    centos.vm.hostname = "fintech-centos"
    centos.vm.network "private_network", ip: "192.168.56.11"
    # Додаток з CentOS - http://localhost:15002/swagger
    centos.vm.network "forwarded_port", guest: 5002, host: 15002

    centos.vm.provider "virtualbox" do |vb|
      vb.name = "FintechStats-CentOS"
      vb.memory = "2048"
      vb.cpus = 2
      vb.gui = true
    end

    centos.vm.provision "shell", inline: <<-SHELL
    
      # Шлях до папки з .csproj файлом всередині VM
      PROJECT_DIR="/vagrant/Zenflow" 
      
      echo "=== Встановлення .NET SDK 8.0 (CentOS) ==="
      sudo dnf install -y dotnet-sdk-8.0

      echo "=== Встановлення PostgreSQL (CentOS) ==="
      sudo dnf install -y postgresql-server postgresql-contrib
      sudo postgresql-setup --initdb || true
      sudo systemctl start postgresql
      sudo systemctl enable postgresql
      
      echo "=== Вимкнення фаєрволу (CentOS) ==="
      sudo systemctl stop firewalld
      sudo systemctl disable firewalld

      echo "=== Налаштування БД ==="
      sudo -u postgres psql -c "CREATE DATABASE fintechstats;" || true
      sudo -u postgres psql -c "CREATE USER fintechuser WITH PASSWORD 'fintechpass';" || true
      sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE fintechstats TO fintechuser;" || true

      echo "=== Збірка та запуск додатку (CentOS) ==="
      cd $PROJECT_DIR
      
      echo "=== Виконуємо dotnet build ==="
      dotnet build
      
      echo "Запускаємо додаток у фоновому режимі на порту 5002"
      nohup dotnet run --urls "http://0.0.0.0:5002" > /home/vagrant/app.log 2>&1 &
      
      echo "✅ CentOS VM готова! Додаток запущено."
      echo "✅ Доступно на вашому комп'ютері: http://localhost:15002/swagger"
    SHELL
  end
end