(function(){
  var s=document.body.style,
      dir='styles/uix/ponykart/',
      kartPairs=['l','c','p','f','r','d','t'].map(function(s){return 'BPs/v-'+s;}).map(function(s){return [s+'0.png',s+'1.png']}),
      kp=kartPairs[~~(Math.random()*kartPairs.length*0.99)],
      bgi=[
        'linear-gradient(120deg, #259 0%, #124 100%)',
        'radial-gradient(circle, rgba(255,255,255,0) 60%, rgba(255,255,255,0.3) 100%)',
        'url('+dir+'bga.png)',
        'url('+dir+kp[0]+')',
        'url('+dir+'BPs/v-dimbox.png)',
        'url('+dir+kp[1]+')',
        'url('+dir+'BPs/v-dimbox.png)',
        'url('+dir+'dirt.png)'
      ],
      bga=[
        'fixed',
        'fixed',
        'scroll',
        'scroll',
        'scroll',
        'scroll',
        'scroll',
        'scroll'
      ],
      bgp=[
        '0px,0px',
        '50% 50%',
        '0px,0px',
        'calc(50% - 1170px / 2 - 2em) 18em',
        'calc(50% - 1170px / 2 - 2em) 18em',
        'calc(50% + 1170px / 2 + 2em) 18em',
        'calc(50% + 1170px / 2 + 2em) 18em',
        '0% 0%'
      ],
      bgs=[
        '100% 100%',
        '100%',
        'auto',
        '30em',
        '30em',
        '30em',
        '30em',
        '100% auto'
      ],
      bgr=[
        'no-repeat',
        'no-repeat',
        'repeat',
        'no-repeat',
        'no-repeat',
        'no-repeat',
        'no-repeat',
        'repeat'
      ]
  s.background=bgi.reverse().join(',');
  s.backgroundPosition=bgp.reverse().join(',');
  s.backgroundSize=bgs.reverse().join(',');
  s.backgroundRepeat=bgr.reverse().join(',');
  s.backgroundAttachment=bga.reverse().join(',');
})()
