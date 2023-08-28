﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ActorLib;

using Akka.Configuration;
using Akka.TestKit.Xunit2;

using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.Utilities;

using Xunit.Abstractions;

namespace ActorLibTest
{
    public abstract class TestKitXunit : TestKit
    {
        protected readonly AkkaService akkaService;

        protected IConfiguration configuration;

        protected readonly ITestOutputHelper output;

        private readonly TextWriter _originalOut;

        private readonly TextWriter _textWriter;

        public TestKitXunit(ITestOutputHelper output) : base(GetConfig())
        {
            this.output = output;
            _originalOut = Console.Out;
            _textWriter = new StringWriter();
            Console.SetOut(_textWriter);

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configuration = configurationBuilder.Build();
            akkaService = new AkkaService();

            // 실 사용서비스에서는 ActorSystem을 생성해야합니다.
            // TestToolKit에서는 테스트검증을 위한 기본 ActorSystem이 생성되어
            // 여기서는 생성된 ActorSystem을 이용합니다.           
            // 시스템생성코드 : akkaService.CreateActorSystem("test");

            akkaService.FromActorSystem(this.Sys);
        }

        public static Config GetConfig()
        {
            return ConfigurationFactory.ParseString(@"
                akka {	
	                loglevel = DEBUG
	                loggers = [""Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog""]                
                }

                custom-dispatcher {
                    type = Dispatcher
                    throughput = 1
                }

                custom-task-dispatcher {
                  type = TaskDispatcher
                  throughput = 1
                }

                fork-join-dispatcher {
                  type = ForkJoinDispatcher
                  throughput = 1
                  dedicated-thread-pool {
                      thread-count = 1
                      deadlock-timeout = 3s
                      threadtype = background
                  }
                }

                synchronized-dispatcher {
                  type = SynchronizedDispatcher
                  throughput = 1
                }

            ");
        }

        protected override void Dispose(bool disposing)
        {
            output.WriteLine(_textWriter.ToString());
            Console.SetOut(_originalOut);
            base.Dispose(disposing);
        }

    }
}
