﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using d60.Cirqus.Events;
using d60.Cirqus.Numbers;
using Newtonsoft.Json;

namespace d60.Cirqus.Serialization
{
    public class Serializer
    {
        readonly TypeAliasBinder _binder;
        readonly JsonSerializerSettings _settings;

        public Serializer(string virtualNamespaceName)
        {
            _binder = new TypeAliasBinder(virtualNamespaceName);
            _settings = new JsonSerializerSettings
            {
                Binder = _binder.AddType(typeof(Metadata)),
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };
        }

        public Serializer AddAliasFor(Type type)
        {
            _binder.AddType(type);
            return this;
        }

        public Serializer AddAliasesFor(params Type [] types)
        {
            return AddAliasesFor((IEnumerable<Type>) types);
        }

        public Serializer AddAliasesFor(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AddAliasFor(type);
            }
            return this;
        }

        public string Serialize(DomainEvent e)
        {
            try
            {
                return JsonConvert.SerializeObject(e, _settings);
            }
            catch (Exception exception)
            {
                throw new SerializationException(string.Format("Could not serialize DomainEvent {0} into JSON!", e), exception);
            }
        }

        public DomainEvent Deserialize(string text)
        {
            try
            {
                return (DomainEvent) JsonConvert.DeserializeObject(text, _settings);
            }
            catch (Exception exception)
            {
                throw new SerializationException(string.Format("Could not deserialize JSON text '{0}' into proper DomainEvent!", text), exception);
            }
        }

        public void EnsureSerializability(DomainEvent domainEvent)
        {
            var firstSerialization = Serialize(domainEvent);

            var secondSerialization = Serialize(Deserialize(firstSerialization));

            if (firstSerialization.Equals(secondSerialization)) return;

            throw new ArgumentException(string.Format(@"Could not properly roundtrip the following domain event: {0}

Result after first serialization:

{1}

Result after roundtripping:

{2}", domainEvent, firstSerialization, secondSerialization));
        }
    }
}