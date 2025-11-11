import { useParamsStore } from '@/hooks/useParamsStore';
import { Button, ButtonGroup } from 'flowbite-react';
import { AiOutlineClockCircle, AiOutlineSortAscending } from 'react-icons/ai';
import { BsFillStopCircleFill, BsStopwatchFill } from 'react-icons/bs';
import { GiFinishLine, GiFlame } from 'react-icons/gi';

const pageSizeButtons = [4, 8, 12];

const orderedButtons = [
  { label: 'Alphabetical', icon: AiOutlineSortAscending, value: 'make' },
  { label: 'End Date', icon: AiOutlineClockCircle, value: 'endingSoon' },
  { label: 'Recently Added', icon: BsFillStopCircleFill, value: 'new' },
];

const filterButtons = [
  { label: 'Live AUctions', icon: GiFlame, value: 'live' },
  { label: 'Ending < 6 hours', icon: GiFinishLine, value: 'endingSoon' },
  { label: 'Completed', icon: BsStopwatchFill, value: 'finished' },
];

export default function Filters() {
  const pageSize = useParamsStore((state) => state.pageSize);
  const setParams = useParamsStore((state) => state.setParams);

  const orderBy = useParamsStore((state) => state.orderBy);
  const filterBy = useParamsStore((state) => state.filterBy);

  return (
    <div className="flex justify-between mb-2">
      <div>
        <span className="uppercase text-sm text-gray-700 mr-2">Filter By</span>
        <ButtonGroup outline>
          {filterButtons.map(({ label, icon: Icon, value }) => (
            <Button
              key={value}
              color={`${filterBy === value ? 'red' : 'gray'}`}
              onClick={() => setParams({ filterBy: value })}
              className="focus:ring-0"
            >
              <Icon className="mr-3 h-4 w-4"></Icon>
              {label}
            </Button>
          ))}
        </ButtonGroup>
      </div>
      <div>
        <span className="uppercase text-sm text-gray-700 mr-2">Order By</span>
        <ButtonGroup outline>
          {orderedButtons.map(({ label, icon: Icon, value }) => (
            <Button
              key={value}
              color={`${orderBy === value ? 'red' : 'gray'}`}
              onClick={() => setParams({ orderBy: value })}
              className="focus:ring-0"
            >
              <Icon className="mr-3 h-4 w-4"></Icon>
              {label}
            </Button>
          ))}
        </ButtonGroup>
      </div>
      <div>
        <span className="uppercase text-sm text-gray-700 mr-2">Page Size</span>
        <ButtonGroup outline>
          {pageSizeButtons.map((value, index) => (
            <Button
              key={index}
              onClick={() => setParams({ pageSize: value })}
              color={`${pageSize === value ? 'red' : 'gray'}`}
              className="focus:ring-0"
            >
              {value}
            </Button>
          ))}
        </ButtonGroup>
      </div>
    </div>
  );
}
