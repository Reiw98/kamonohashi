import Index from '@/views/system-setting/node/Index'
import Edit from '@/views/system-setting/node/Edit'

export default [
  {
    path: '/node',
    name: 'Node,',
    component: Index,
    children: [
      {
        path: 'edit/:id?',
        component: Edit,
        props: true,
      },
    ],
  },
]
