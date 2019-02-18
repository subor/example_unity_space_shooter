#!groovy
pipeline {
	agent {
		node{
			label 'unity'
		}
	}
	
	parameters {
		booleanParam(name: 'CLEAN_BUILD', defaultValue: true, description: 'Options for doing a clean build when is true')
		booleanParam(name: 'CODE_SIGN', defaultValue: true, description: 'Options to sign the build when is true')
		string(name: 'REF_BUILD_NUMBER', defaultValue: '', description: 'Specified build number to copy dependencies(E.g.: 22, using latest build when empty)')
		string(name: 'JOB_NAME', defaultValue: 'RUYI-Platform-CleanBuild', description: 'Specified job name to copy dependencies')
		booleanParam(name: 'MAIL_ON_FAILED', defaultValue: true, description: 'Options for sending mail when failed')
	}
	
	options{
		//Set a timeout period for the Pipeline run, after which Jenkins should abort the Pipeline.
		timeout(time: 2, unit: 'HOURS')	
		//Persist artifacts and console output for the specific number of recent Pipeline runs. 
		buildDiscarder(logRotator(numToKeepStr: '30', artifactNumToKeepStr: '30'))
		//Skip checking out code from source control by default in the agent directive.
		skipDefaultCheckout()
		//Disallow concurrent executions of the Pipeline.
		//disableConcurrentBuilds()
	}
	
	environment{
		//Windows command prompt encoding(65001=UTF8;437=US.English;936=GBK)
		WIN_CMD_ENCODING = '65001'
		//Unity Engine root
		UE_ROOT = "C:/Jenkins/tools/Unity/Editor"
		//Temp folder
		TEMP_DIR = 'temp'
		//Root directory for all build tools
		WIN32_TOOLS = "c:/jenkins/tools"
		//Ruyi SDK CPP folder
		RUYI_SDK_CPP = "${TEMP_DIR}\\RuyiSDK.nf2.0"		
		//RuyiSDKUnityCS Root
		RuyiSDKUnityCS = "RuyiSDKUnityCS"
		//Ruyi SDK DEMO folder
		RUYI_SDK_DEMO = "${RuyiSDKUnityCS}\\jade\\sdk\\RuyiSDKUnity"
		//Ruyi DevTools folder
		RUYI_DEV_ROOT = "${TEMP_DIR}\\DevToolsInternal"
		//Unity Demo Root
		DEMO_PROJECT_ROOT = "space_shooter"
		//CodeSigning
		CODESIGNING_HOME = "${WIN32_TOOLS}/CodeSigning"
		//Sign root
		SIGN_ROOT = "${workspace}\\${DEMO_PROJECT_ROOT}\\Pack"
		
		//ASSETS_PLUGINS folder
		ASSETS_PLUGINS="${DEMO_PROJECT_ROOT}\\Assets\\plugins\\x64"
		//ASSETS_SCRIPTS folder
		ASSETS_SCRIPTS="${DEMO_PROJECT_ROOT}\\Assets\\RuyiNet\\Scripts"
		
		//Unity packed target
		COOKED_ROOT = "${DEMO_PROJECT_ROOT}\\Pack"
		//File path for saving commit id
		COMMIT_ID_FILE = "${COOKED_ROOT}\\commit-id"
		//Mail recipient on failed
		MAIL_RECIPIENT = 'sw-engr@playruyi.com,cc:chris.zhang@playruyi.com'
	}
		
	stages {
		stage('Initialize') {
			steps{
				//clean workspace
				script{
					//Never did a clean build in multi-branch pipeline task lol
					if(env.BRANCH_NAME == null && params.CLEAN_BUILD){
						deleteDir()
					}else{
						step([$class: 'WsCleanup', patterns: [[pattern: "**/Pack/space_shooter/**/**", type: 'INCLUDE'],[pattern: "**/.git/*.lock", type: 'INCLUDE']]])
					}
									
					checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches:  scm.branches,
						 doGenerateSubmoduleConfigurations: false, extensions: [
							[$class: 'RelativeTargetDirectory', relativeTargetDir: "${DEMO_PROJECT_ROOT}"],
							[$class: 'CleanBeforeCheckout'],
							[$class: 'CheckoutOption', timeout: 60],
							[$class: 'CloneOption', honorRefspec: true, depth: 0, noTags: true, reference: '', shallow: true,timeout: 60]
						 ], 
						 submoduleCfg: [], 
						 userRemoteConfigs: [[credentialsId: scm.userRemoteConfigs[0].credentialsId, url: scm.userRemoteConfigs[0].url]]
					]
					
					checkout changelog: false, poll: false, scm: [$class: 'GitSCM', branches:  [[name: 'development']],
						 doGenerateSubmoduleConfigurations: false, extensions: [
							[$class: 'SparseCheckoutPaths', sparseCheckoutPaths: [[path: 'jade/sdk/RuyiSDKUnity']]],
							[$class: 'RelativeTargetDirectory', relativeTargetDir: "${RuyiSDKUnityCS}"],
							[$class: 'CleanBeforeCheckout'],
							[$class: 'CheckoutOption', timeout: 60],
							[$class: 'CloneOption', honorRefspec: true, depth: 0, noTags: true, reference: '', shallow: true,timeout: 60]
						 ], 
						 submoduleCfg: [], 
						 userRemoteConfigs: [[credentialsId:"credential_access_bitbucket", url: "http://bitbucket:7990/scm/ruyi/jade.git"]]
					]
				}
			}
			
			post {
				success {
					stage_success env.STAGE_NAME
				}
				failure {
					stage_failed env.STAGE_NAME
				}
			}
		}
		
		stage('Copy dependencies'){
			steps{
				script{
					def jobName = params.JOB_NAME
					def sel = [$class:'StatusBuildSelector',stable:false]
					
					if(params.REF_BUILD_NUMBER?.trim())
						sel = specific("${params.REF_BUILD_NUMBER}")
						
					step([$class:'CopyArtifact',filter:'RuyiSDK.nf2.0/**/*,DevToolsInternal/**/*', projectName: 'RUYI-Platform-CleanBuild', selector: sel, target:"${TEMP_DIR}"])
					bat """
						xcopy ${TEMP_DIR}\\RuyiSDK.nf2.0\\* ${ASSETS_PLUGINS}\\* /s /i /y
						xcopy ${RUYI_SDK_DEMO}\\* ${ASSETS_SCRIPTS}\\* /s /i /y
						
					"""
				}
			}
			
			post {
				success {
					stage_success env.STAGE_NAME
				}
				failure {
					stage_failed env.STAGE_NAME
				}
			}
		}
		
		
		stage('Cook Demo'){
			steps{
				//Cook
				bat """
					chcp ${WIN_CMD_ENCODING}
					start /wait ${UE_ROOT.replaceAll('/','\\\\')}\\Unity.exe -quit -batchmode -projectPath "${workspace}\\${DEMO_PROJECT_ROOT}\\" -buildWindows64Player Pack\\space_shooter\\space_shooter.exe
				"""
				script {
					if (params.CODE_SIGN) {
						codeSign()
					}
				}
			}
			
			post {
				success {
					stage_success env.STAGE_NAME
				}
				failure {
					stage_failed env.STAGE_NAME
				}
			}
		}

		stage('Archive'){
			steps{
				script{
					bat """
						pushd ${DEMO_PROJECT_ROOT}
						git rev-parse HEAD > ${workspace.replaceAll('/','\\\\')}\\${COMMIT_ID_FILE}
						popd
						exit 0
					"""

					echo 'Start archiving artifacts ...'
					archiveArtifacts artifacts: "${COOKED_ROOT}/**/**", onlyIfSuccessful: true
				}
			}
			
			post {
				success {
					stage_success env.STAGE_NAME
				}
				failure {
					stage_failed env.STAGE_NAME
				}
			}
		}
				
		stage('Finalize'){
			steps{
				dir(TEMP_DIR){
					deleteDir()
				}
			}
			
			post {
				success {
					stage_success env.STAGE_NAME
				}
				failure {
					stage_failed env.STAGE_NAME
				}
			}
		}
	}
	
}

void stage_success(stage){
	echo "[${stage}] stage successfully completed"
}

void stage_failed(stage){
	echo "[${stage}] stage failed"
	
	if(env.FAILURE_STAGE==null)
		env.FAILURE_STAGE = stage
}

void codeSign(){
	echo 'Start processing code signing'
	dir(CODESIGNING_HOME){
		withCredentials([[$class: 'UsernamePasswordMultiBinding', credentialsId: 'credentials_ruyi_codesign',
				usernameVariable: 'USERNAME', passwordVariable: 'PASSWORD']]) {
			bat """
				echo %date% %time% >cgrecord.log
				for /f %%i in ('dir ${SIGN_ROOT.replaceAll('/','\\\\')}\\*.exe /s /b') do x64\\signtool.exe sign /f RUYI-CERT.pfx /p %PASSWORD% /fd sha256 /tr http://sha256timestamp.ws.symantec.com/sha256/timestamp /as /v %%i
				echo %date% %time% >>cgrecord.log
				for /f %%i in ('dir ${SIGN_ROOT.replaceAll('/','\\\\')}\\*.dll /s /b') do x64\\signtool.exe sign /f RUYI-CERT.pfx /p %PASSWORD% /fd sha256 /tr http://sha256timestamp.ws.symantec.com/sha256/timestamp /as /v %%i
				echo %date% %time% >>cgrecord.log
				for /f %%i in ('dir ${SIGN_ROOT.replaceAll('/','\\\\')}\\*.sys /s /b') do x64\\signtool.exe sign /f RUYI-CERT.pfx /p %PASSWORD% /fd sha256 /tr http://sha256timestamp.ws.symantec.com/sha256/timestamp /as /v %%i
				echo %date% %time% >>cgrecord.log
				for /f %%i in ('dir ${SIGN_ROOT.replaceAll('/','\\\\')}\\*.cat /s /b') do x64\\signtool.exe sign /f RUYI-CERT.pfx /p %PASSWORD% /fd sha256 /tr http://sha256timestamp.ws.symantec.com/sha256/timestamp /as /v %%i
				echo %date% %time% >>cgrecord.log
				for /f %%i in ('dir ${SIGN_ROOT.replaceAll('/','\\\\')}\\*.ocx /s /b') do x64\\signtool.exe sign /f RUYI-CERT.pfx /p %PASSWORD% /fd sha256 /tr http://sha256timestamp.ws.symantec.com/sha256/timestamp /as /v %%i
				echo %date% %time% >>cgrecord.log
			"""
		}
	}
}