# To rebuild everything and create the msi installers run:
#
#   rake rebuild_setup
#
# To rebuild everything and install Git Extensions run:
#
#   rake install
#
# To change the version number of the installer, create a
# git tag with the required number. If the current HEAD does
# not have a tag then git will create a description based on
# the last tag it is able to find in the history.
#
# Git may, for example, describe a version such as 1.91-79-g05ccafd.
# This means that the current HEAD is 79 commits past the 1.91
# release and its hash starts with 05ccafd.
#
VERSION = `git describe --tags`.strip

MSBUILD = "#{ENV['WinDir']}/Microsoft.NET/Framework/v3.5/msbuild.exe"
CSPROJECTS = "**/*.csproj"
MSIPROJECT = "Setup/Setup.wixproj"
SHELL_EXT = "SimpleExt/SimpleExt.sln"
CONFIG = "Release"

ENV["SkipShellExtRegistration"] = "yes"

# don't write the command to be executed by 'sh' to stdout
verbose(false)

def tasks
  task :default => :compile

  task :rebuild => [:clean, :compile]
  task :compile => msbuild_all('build')
  task :clean => msbuild_all('clean')
  
  task :rebuild_setup => [:clean_setup, :setup]
  task :setup => [:normal_setup, :complete_setup]
  task :complete_setup => wix_complete('build')
  task :normal_setup => wix('build')
  
  task :clean_setup do
    FileUtils.rm_rf 'Setup/bin'
    FileUtils.rm_rf 'Setup/obj'
  end

  task :install => [:rebuild, :clean_setup, :normal_setup] do
    install_msi
  end
end

def install_msi(suffix=nil)
  msi = "Setup\\bin\\#{CONFIG}\\#{msi_name}#{suffix}.msi"
  sh "msiexec.exe /i #{msi}"
end

def msbuild_all(target)
  msbuild_csprojects(target) <<
  msbuild(target, SHELL_EXT, {"Platform" => "Win32"}) <<
  msbuild(target, SHELL_EXT, {"Platform" => "x64"}) 
end

def msbuild_csprojects(target)
  FileList[CSPROJECTS].map {|x| msbuild(target, x)}
end

def wix_complete(target)
  wix(target, 'Complete', true)
end

def wix(target, suffix=nil, extra_software=false)
  msbuild(target, MSIPROJECT, {
    "OutputName" => msi_name(suffix),
    "Version" => VERSION,
    "NumericVersion" => msi_numeric_version,
    "IncludeRequiredSoftware" => if extra_software then 1 else 0 end,
  }, suffix)
end

def msi_name(suffix='')
  "GitExtensions#{VERSION.gsub(/-g.*/, '').gsub(/\./,'')}Setup#{suffix}"
end

def msi_numeric_version(suffix='')
  VERSION.gsub(/-/,'.').gsub(/\.g.*/,'')
end

def msbuild(target, project, props={}, suffix=nil)
  platform = ":#{props['Platform']}" if props.key?('Platform')
  suffix = ":#{suffix}" if suffix != nil

  task "#{target}#{platform}#{suffix}:#{project}" do
    run_msbuild(target, project, props)
  end
end

def run_msbuild(target, project, props)
  cmd = "#{MSBUILD} "
  cmd += "/nologo /m /v:m "
  props.each{|k,v| cmd += "/p:#{k}=\"#{v}\" "}
  cmd += "/p:Configuration=#{CONFIG} "
  cmd += "/t:#{target} #{project}"

  prop_desc = props.to_a.map{|x| "#{x[0]} = #{x[1]}"}.join(', ')
  desc = project
  desc += " (#{prop_desc})" if prop_desc.size != 0

  if target == 'clean' then
    puts "Cleaning #{desc}"
  else
    puts "Building #{desc}"
  end

  startTime = Time.now
  sh cmd
  endTime = Time.now
end

tasks
